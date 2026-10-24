using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class GameReleaseFileServiceFixture : CoreTest<GameReleaseFileService>
    {
        private string _folder;
        private Dictionary<string, string> _contents;
        private Dictionary<string, long> _sizes;

        [SetUp]
        public void Setup()
        {
            _folder = @"C:\Downloads\Release".AsOsAgnostic();
            _contents = new Dictionary<string, string>();
            _sizes = new Dictionary<string, long>();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(It.IsAny<string>()))
                  .Returns<string>(p => _contents.ContainsKey(p) || _sizes.ContainsKey(p));

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.ReadAllText(It.IsAny<string>()))
                  .Returns<string>(p => _contents.TryGetValue(p, out var c) ? c : string.Empty);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFileSize(It.IsAny<string>()))
                  .Returns<string>(p => _sizes.TryGetValue(p, out var size) ? size : 1);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);
        }

        private string GivenFile(string relativePath, string content = null, long size = 1)
        {
            var path = Path.Combine(_folder, relativePath.AsOsAgnostic());

            if (content != null)
            {
                _contents[path] = content;
            }

            _sizes[path] = size;

            return path;
        }

        private void GivenFolderFiles(params string[] files)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(It.IsAny<string>(), It.IsAny<bool>()))
                  .Returns(files);
        }

        [Test]
        public void should_use_cue_as_primary_of_bin_cue_release()
        {
            var bin1 = GivenFile("Game (USA) (Track 1).bin", size: 500);
            var bin2 = GivenFile("Game (USA) (Track 2).bin", size: 20);
            var cue = GivenFile("Game (USA).cue", "FILE \"Game (USA) (Track 1).bin\" BINARY\nFILE \"Game (USA) (Track 2).bin\" BINARY\n");

            var release = Subject.GetRelease(new List<string> { bin1, cue, bin2 });

            release.PrimaryFile.Should().Be(cue);
            release.CompanionFiles.Should().BeEquivalentTo(new[] { bin1, bin2 });
            release.Root.Should().Be(_folder);
            release.IsInstaller.Should().BeFalse();
        }

        [Test]
        public void should_use_m3u_as_primary_of_multi_disc_release()
        {
            var cue1 = GivenFile(@"Disc 1\Game (USA) (Disc 1).cue", "FILE \"Game (USA) (Disc 1).bin\" BINARY\n");
            var bin1 = GivenFile(@"Disc 1\Game (USA) (Disc 1).bin");
            var cue2 = GivenFile(@"Disc 2\Game (USA) (Disc 2).cue", "FILE \"Game (USA) (Disc 2).bin\" BINARY\n");
            var bin2 = GivenFile(@"Disc 2\Game (USA) (Disc 2).bin");
            var m3u = GivenFile("Game (USA).m3u", "Disc 1/Game (USA) (Disc 1).cue\nDisc 2/Game (USA) (Disc 2).cue\n".Replace('/', Path.DirectorySeparatorChar));

            var release = Subject.GetRelease(new List<string> { cue1, bin1, cue2, bin2, m3u });

            release.PrimaryFile.Should().Be(m3u);
            release.CompanionFiles.Should().BeEquivalentTo(new[] { cue1, bin1, cue2, bin2 });
        }

        [Test]
        public void should_use_first_disc_without_playlist()
        {
            var disc2 = GivenFile("Final Fantasy VII (USA) (Disc 2).chd", size: 700);
            var disc1 = GivenFile("Final Fantasy VII (USA) (Disc 1).chd", size: 600);
            var disc3 = GivenFile("Final Fantasy VII (USA) (Disc 3).chd", size: 650);

            var release = Subject.GetRelease(new List<string> { disc2, disc1, disc3 });

            release.PrimaryFile.Should().Be(disc1);
            release.CompanionFiles.Should().HaveCount(2);
        }

        [Test]
        public void should_use_gdi_as_primary()
        {
            var gdi = GivenFile("Shenmue (Europe) (Disc 1).gdi", "3\n1 0 4 2352 track01.bin 0\n2 600 0 2352 track02.raw 0\n3 45000 4 2352 track03.bin 0\n");
            var track1 = GivenFile("track01.bin");
            var track2 = GivenFile("track02.raw");
            var track3 = GivenFile("track03.bin");

            var release = Subject.GetRelease(new List<string> { track1, gdi, track3 });

            release.PrimaryFile.Should().Be(gdi);
            release.CompanionFiles.Should().BeEquivalentTo(new[] { track1, track2, track3 });
        }

        [Test]
        public void should_take_whole_folder_of_pc_installer()
        {
            var setup = GivenFile("setup_hollow_knight_1.5.78.11833_(44711).exe");
            var bin = GivenFile("setup_hollow_knight_1.5.78.11833_(44711)-1.bin");
            var redist = GivenFile(@"Redist\vcredist.exe");
            var nfo = GivenFile("info.nfo");

            GivenFolderFiles(setup, bin, redist, nfo, Path.Combine(_folder, "Thumbs.db"));

            var release = Subject.GetRelease(new List<string> { setup, bin, redist });

            release.PrimaryFile.Should().Be(setup);
            release.IsInstaller.Should().BeTrue();
            release.CompanionFiles.Should().BeEquivalentTo(new[] { bin, redist, nfo });
        }

        [Test]
        public void should_prefer_base_game_over_update_and_dlc()
        {
            var update = GivenFile("Game [0100ABCD12345800][v65536][UPDATE].nsp", size: 900);
            var game = GivenFile("Game [0100ABCD12345000][v0].nsp", size: 500);
            var dlc = GivenFile("Game Expansion [0100ABCD12346001][v0][DLC].nsp", size: 1000);

            var release = Subject.GetRelease(new List<string> { update, dlc, game });

            release.PrimaryFile.Should().Be(game);
            release.CompanionFiles.Should().BeEquivalentTo(new[] { update, dlc });
        }

        [Test]
        public void should_prefer_image_over_installer_and_companion_only_files()
        {
            var iso = GivenFile("codex-game.iso", size: 100);
            var bin = GivenFile("something.bin", size: 1000);

            Subject.GetRelease(new List<string> { bin, iso }).PrimaryFile.Should().Be(iso);
        }

        [Test]
        public void should_ignore_junk_files()
        {
            var rom = GivenFile("Super Mario World (USA).sfc");
            var par = GivenFile("Super Mario World (USA).par2");

            var release = Subject.GetRelease(new List<string> { rom, par });

            release.PrimaryFile.Should().Be(rom);
            release.CompanionFiles.Should().BeEmpty();
        }

        [Test]
        public void should_group_loose_roms_as_separate_releases()
        {
            var rom1 = GivenFile("Super Mario World (USA).sfc");
            var rom2 = GivenFile("Super Metroid (USA).sfc");
            var rom3 = GivenFile("Chrono Trigger (USA).sfc");

            var releases = Subject.GroupReleases(new List<string> { rom1, rom2, rom3 });

            releases.Should().HaveCount(3);
            releases.Should().OnlyContain(r => !r.CompanionFiles.Any());
        }

        [Test]
        public void should_group_disc_images_and_cue_sheets()
        {
            var cue = GivenFile("Game A (USA).cue", "FILE \"Game A (USA).bin\" BINARY\n");
            var bin = GivenFile("Game A (USA).bin");
            var disc1 = GivenFile("Game B (USA) (Disc 1).chd");
            var disc2 = GivenFile("Game B (USA) (Disc 2).chd");
            var rom = GivenFile("Game C (USA).gba");

            var releases = Subject.GroupReleases(new List<string> { cue, bin, disc1, disc2, rom });

            releases.Should().HaveCount(3);
            releases.Single(r => r.PrimaryFile == cue).CompanionFiles.Should().BeEquivalentTo(new[] { bin });
            releases.Single(r => r.PrimaryFile == disc1).CompanionFiles.Should().BeEquivalentTo(new[] { disc2 });
            releases.Single(r => r.PrimaryFile == rom).CompanionFiles.Should().BeEmpty();
        }

        [Test]
        public void should_find_companions_of_cue_sheet()
        {
            var cue = GivenFile("Game (USA).cue", "FILE \"Game (USA) (Track 1).bin\" BINARY\nFILE \"Game (USA) (Track 2).bin\" BINARY\n");
            var bin1 = GivenFile("Game (USA) (Track 1).bin");
            var bin2 = GivenFile("Game (USA) (Track 2).bin");
            var other = GivenFile("Other Game (USA).iso");

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(_folder, false))
                  .Returns(new[] { cue, bin1, bin2, other });

            Subject.FindCompanions(cue).Should().BeEquivalentTo(new[] { bin1, bin2 });
        }
    }
}
