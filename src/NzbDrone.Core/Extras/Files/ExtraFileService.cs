using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Games;
using NzbDrone.Core.Games.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Files
{
    public interface IExtraFileService<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        List<TExtraFile> GetFilesByGame(int gameId);
        List<TExtraFile> GetFilesByGameFile(int gameFileId);
        TExtraFile FindByPath(int gameId, string path);
        void Upsert(TExtraFile extraFile);
        void Upsert(List<TExtraFile> extraFiles);
        void Delete(int id);
        void DeleteMany(IEnumerable<int> ids);
    }

    public abstract class ExtraFileService<TExtraFile> : IExtraFileService<TExtraFile>,
                                                         IHandle<GameFileDeletedEvent>,
                                                         IHandleAsync<GamesDeletedEvent>
        where TExtraFile : ExtraFile, new()
    {
        private readonly IExtraFileRepository<TExtraFile> _repository;
        private readonly IGameService _gameService;
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly Logger _logger;

        public ExtraFileService(IExtraFileRepository<TExtraFile> repository,
                                IGameService gameService,
                                IDiskProvider diskProvider,
                                IRecycleBinProvider recycleBinProvider,
                                Logger logger)
        {
            _repository = repository;
            _gameService = gameService;
            _diskProvider = diskProvider;
            _recycleBinProvider = recycleBinProvider;
            _logger = logger;
        }

        public List<TExtraFile> GetFilesByGame(int gameId)
        {
            return _repository.GetFilesByGame(gameId);
        }

        public List<TExtraFile> GetFilesByGameFile(int gameFileId)
        {
            return _repository.GetFilesByGameFile(gameFileId);
        }

        public TExtraFile FindByPath(int gameId, string path)
        {
            return _repository.FindByPath(gameId, path);
        }

        public void Upsert(TExtraFile extraFile)
        {
            Upsert(new List<TExtraFile> { extraFile });
        }

        public void Upsert(List<TExtraFile> extraFiles)
        {
            extraFiles.ForEach(m =>
            {
                m.LastUpdated = DateTime.UtcNow;

                if (m.Id == 0)
                {
                    m.Added = m.LastUpdated;
                }
            });

            _repository.InsertMany(extraFiles.Where(m => m.Id == 0).ToList());
            _repository.UpdateMany(extraFiles.Where(m => m.Id > 0).ToList());
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            _repository.DeleteMany(ids);
        }

        public void HandleAsync(GamesDeletedEvent message)
        {
            _repository.DeleteForGames(message.Games.Select(m => m.Id).ToList());
        }

        // Handled synchronously: when a game file is upgraded its extras (companion tracks, discs) are removed before the
        // new release's files are put in place, often at the same paths. Done later, it would delete the new files.
        public void Handle(GameFileDeletedEvent message)
        {
            var gameFile = message.GameFile;

            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing game file from DB as part of cleanup routine, not deleting extra files from disk.");
            }
            else
            {
                var game = _gameService.GetGame(message.GameFile.GameId);
                var extras = _repository.GetFilesByGameFile(gameFile.Id);

                // A path that's also an extra of another game file is that file's now, it isn't deleted
                var otherPaths = _repository.GetFilesByGame(game.Id)
                    .Where(e => e.GameFileId != gameFile.Id)
                    .Select(e => Path.Combine(game.Path, e.RelativePath))
                    .ToHashSet(PathEqualityComparer.Instance);

                foreach (var extra in extras)
                {
                    var path = Path.Combine(game.Path, extra.RelativePath);

                    if (otherPaths.Contains(path))
                    {
                        _logger.Debug("Extra file {0} belongs to another game file, not deleting it", path);
                        continue;
                    }

                    try
                    {
                        if (_diskProvider.FileExists(path))
                        {
                            // Send to the recycling bin so they can be recovered if necessary
                            var subfolder = _diskProvider.GetParentFolder(game.Path).GetRelativePath(_diskProvider.GetParentFolder(path));
                            _recycleBinProvider.DeleteFile(path, subfolder);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Unable to delete extra file {0}", path);
                    }
                }
            }

            _logger.Debug("Deleting Extra from database for game file: {0}", gameFile);
            _repository.DeleteForGameFile(gameFile.Id);
        }
    }
}
