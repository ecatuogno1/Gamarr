using System;
using System.Collections.Generic;
using System.Linq;
using Equ;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.FileList
{
    public class FileListSettingsValidator : AbstractValidator<FileListSettings>
    {
        public FileListSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Passkey).NotEmpty();

            RuleFor(c => c.Categories).NotEmpty();

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());
        }
    }

    public class FileListSettings : PropertywiseEquatable<FileListSettings>, ITorrentIndexerSettings
    {
        private static readonly FileListSettingsValidator Validator = new();

        public FileListSettings()
        {
            BaseUrl = "https://filelist.io";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;

            Categories = new[]
            {
                (int)FileListCategories.PcGames,
                (int)FileListCategories.ConsoleGames
            };

            MultiLanguages = Array.Empty<int>();
            FailDownloads = Array.Empty<int>();
            RequiredFlags = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(1, Label = "IndexerSettingsPasskey", Privacy = PrivacyLevel.ApiKey)]
        public string Passkey { get; set; }

        [FieldDefinition(2, Label = "IndexerSettingsApiUrl", Advanced = true, HelpText = "IndexerSettingsApiUrlHelpText")]
        public string BaseUrl { get; set; }

        [FieldDefinition(3, Label = "IndexerSettingsCategories", Type = FieldType.Select, SelectOptions = typeof(FileListCategories), HelpText = "IndexerFileListSettingsCategoriesHelpText")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(4, Type = FieldType.Number, Label = "IndexerSettingsMinimumSeeders", HelpText = "IndexerSettingsMinimumSeedersHelpText", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(5)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new();

        [FieldDefinition(6, Type = FieldType.Checkbox, Label = "IndexerSettingsRejectBlocklistedTorrentHashes", HelpText = "IndexerSettingsRejectBlocklistedTorrentHashesHelpText", Advanced = true)]
        public bool RejectBlocklistedTorrentHashesWhileGrabbing { get; set; }

        [FieldDefinition(7, Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), Label = "IndexerSettingsMultiLanguageRelease", HelpText = "IndexerSettingsMultiLanguageReleaseHelpText", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(8, Type = FieldType.Select, SelectOptions = typeof(FailDownloads), Label = "IndexerSettingsFailDownloads", HelpText = "IndexerSettingsFailDownloadsHelpText", Advanced = true)]
        public IEnumerable<int> FailDownloads { get; set; }

        [FieldDefinition(9, Type = FieldType.Select, SelectOptions = typeof(IndexerFlags), Label = "IndexerSettingsRequiredFlags", HelpText = "IndexerSettingsRequiredFlagsHelpText", HelpLink = "https://github.com/ecatuogno1/Gamarr/wiki/settings#indexer-flags", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    // FileList's own category ids (the same ones Prowlarr and Jackett map to Newznab categories)
    public enum FileListCategories
    {
        [FieldOption(Label = "Jocuri PC (PC Games)")]
        PcGames = 9,
        [FieldOption(Label = "Jocuri Console (Console Games)")]
        ConsoleGames = 10,
        [FieldOption(Label = "Programe (Software)")]
        Software = 8,
        [FieldOption(Label = "Linux")]
        Linux = 17,
        [FieldOption(Label = "Mobile")]
        Mobile = 22,
        [FieldOption(Label = "Diverse (Other)")]
        Other = 18
    }

    public static class FileListCategoryMapping
    {
        private static readonly Dictionary<int, int> NewznabCategories = new()
        {
            { (int)FileListCategories.PcGames, NewznabStandardCategory.PCGames },
            { (int)FileListCategories.ConsoleGames, NewznabStandardCategory.Console },
            { (int)FileListCategories.Software, NewznabStandardCategory.PC },
            { (int)FileListCategories.Linux, NewznabStandardCategory.PCIso },
            { (int)FileListCategories.Mobile, NewznabStandardCategory.PCMobileOther }
        };

        public static int? ToNewznabCategory(int fileListCategory)
        {
            return NewznabCategories.TryGetValue(fileListCategory, out var category) ? category : null;
        }

        // The API returns the category name ("Jocuri PC"), not its id
        public static List<int> ToNewznabCategories(string fileListCategory)
        {
            var result = new List<int>();

            if (string.IsNullOrWhiteSpace(fileListCategory))
            {
                return result;
            }

            var category = fileListCategory.Trim();

            if (category.Equals("Jocuri PC", StringComparison.InvariantCultureIgnoreCase))
            {
                result.Add(NewznabStandardCategory.PCGames);
            }
            else if (category.Equals("Jocuri Console", StringComparison.InvariantCultureIgnoreCase))
            {
                result.Add(NewznabStandardCategory.Console);
            }

            return result;
        }

        // The configured FileList categories that match the platform's Newznab categories, or all of them if none do
        public static List<int> GetSearchCategories(IEnumerable<int> configuredCategories, IEnumerable<int> platformCategories)
        {
            var configured = configuredCategories.Distinct().ToList();
            var platform = platformCategories?.ToList() ?? new List<int>();

            if (platform.Count == 0)
            {
                return configured;
            }

            var matching = configured.Where(c =>
            {
                var newznab = ToNewznabCategory(c);

                return newznab.HasValue && platform.Any(p => NewznabStandardCategory.Covers(newznab.Value, p) || NewznabStandardCategory.Covers(p, newznab.Value));
            }).ToList();

            return matching.Count > 0 ? matching : configured;
        }
    }
}
