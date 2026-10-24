using System;
using System.Collections.Generic;
using System.IO;
using Gamarr.Api.V3.CustomFormats;
using Gamarr.Http.REST;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace Gamarr.Api.V3.GameFiles
{
    public class GameFileResource : RestResource
    {
        public int GameId { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }
        public string Edition { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int? CustomFormatScore { get; set; }
        public int? IndexerFlags { get; set; }
        public List<Region> Regions { get; set; }
        public string Version { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public int DiscNumber { get; set; }
        public int DiscCount { get; set; }

        public string OriginalFilePath { get; set; }
        public bool QualityCutoffNotMet { get; set; }
    }

    public static class GameFileResourceMapper
    {
        private static GameFileResource ToResource(this GameFile model)
        {
            if (model == null)
            {
                return null;
            }

            return new GameFileResource
            {
                Id = model.Id,

                GameId = model.GameId,
                RelativePath = model.RelativePath,

                // Path
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                IndexerFlags = (int)model.IndexerFlags,
                Quality = model.Quality,
                Languages = model.Languages,
                ReleaseGroup = model.ReleaseGroup,
                Edition = model.Edition,
                Regions = model.Regions,
                Version = model.Version,
                ReleaseType = model.ReleaseType,
                DiscNumber = model.DiscNumber,
                DiscCount = model.DiscCount,
                OriginalFilePath = model.OriginalFilePath
            };
        }

        public static GameFileResource ToResource(this GameFile model, NzbDrone.Core.Games.Game game, IUpgradableSpecification upgradableSpecification, ICustomFormatCalculationService formatCalculationService)
        {
            if (model == null)
            {
                return null;
            }

            var resource = new GameFileResource
            {
                Id = model.Id,

                GameId = model.GameId,
                RelativePath = model.RelativePath,
                Path = Path.Combine(game.Path, model.RelativePath),
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                Quality = model.Quality,
                Languages = model.Languages,
                Edition = model.Edition,
                ReleaseGroup = model.ReleaseGroup,
                Regions = model.Regions,
                Version = model.Version,
                ReleaseType = model.ReleaseType,
                DiscNumber = model.DiscNumber,
                DiscCount = model.DiscCount,
                QualityCutoffNotMet = upgradableSpecification?.QualityCutoffNotMet(game.QualityProfile, model.Quality) ?? false,
                OriginalFilePath = model.OriginalFilePath,
                IndexerFlags = (int)model.IndexerFlags
            };

            if (formatCalculationService != null)
            {
                model.Game = game;
                var customFormats = formatCalculationService?.ParseCustomFormat(model, model.Game);
                var customFormatScore = game?.QualityProfile?.CalculateCustomFormatScore(customFormats) ?? 0;

                resource.CustomFormats = customFormats.ToResource(false);
                resource.CustomFormatScore = customFormatScore;
            }

            return resource;
        }
    }
}
