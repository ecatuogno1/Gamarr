using System;
using System.IO;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.EmulationStation
{
    public class EmulationStationSettingsValidator : AbstractValidator<EmulationStationMetadataSettings>
    {
        public EmulationStationSettingsValidator()
        {
            RuleFor(c => c.GamelistDirectory)
                .NotEmpty()
                .When(c => c.Gamelist && c.GamelistLocation == (int)EmulationStationGamelistLocation.EsDeGamelists);

            RuleFor(c => c.MediaDirectory)
                .NotEmpty()
                .When(c => c.GameImages && c.GamelistLocation == (int)EmulationStationGamelistLocation.EsDeGamelists);
        }
    }

    public enum EmulationStationGamelistLocation
    {
        // <platform folder>/gamelist.xml, images in the game folder referenced from the gamelist
        [FieldOption(Label = "ROM folder (RetroPie, Batocera, ES-DE legacy)")]
        RomFolder = 0,

        // <gamelists directory>/<system>/gamelist.xml, images in ES-DE's downloaded_media layout
        [FieldOption(Label = "ES-DE gamelists directory")]
        EsDeGamelists = 1
    }

    public class EmulationStationMetadataSettings : IProviderConfig
    {
        public const string DefaultGamelistDirectory = "~/ES-DE/gamelists";
        public const string DefaultMediaDirectory = "~/ES-DE/downloaded_media";

        private static readonly EmulationStationSettingsValidator Validator = new();

        public EmulationStationMetadataSettings()
        {
            Gamelist = true;
            GameImages = true;
            GamelistLocation = (int)EmulationStationGamelistLocation.RomFolder;
            GamelistDirectory = DefaultGamelistDirectory;
            MediaDirectory = DefaultMediaDirectory;
        }

        [FieldDefinition(0, Label = "MetadataSettingsEmulationStationGamelist", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataSettingsEmulationStationGamelistHelpText")]
        public bool Gamelist { get; set; }

        [FieldDefinition(1, Label = "MetadataSettingsEmulationStationGamelistLocation", Type = FieldType.Select, SelectOptions = typeof(EmulationStationGamelistLocation), Section = MetadataSectionType.Metadata, HelpText = "MetadataSettingsEmulationStationGamelistLocationHelpText")]
        public int GamelistLocation { get; set; }

        [FieldDefinition(2, Label = "MetadataSettingsEmulationStationGamelistDirectory", Type = FieldType.Path, Section = MetadataSectionType.Metadata, HelpText = "MetadataSettingsEmulationStationGamelistDirectoryHelpText")]
        public string GamelistDirectory { get; set; }

        [FieldDefinition(3, Label = "MetadataSettingsGameImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "MetadataSettingsEmulationStationImagesHelpText")]
        public bool GameImages { get; set; }

        [FieldDefinition(4, Label = "MetadataSettingsEmulationStationMediaDirectory", Type = FieldType.Path, Section = MetadataSectionType.Image, HelpText = "MetadataSettingsEmulationStationMediaDirectoryHelpText")]
        public string MediaDirectory { get; set; }

        public bool UseEsDeDirectories => GamelistLocation == (int)EmulationStationGamelistLocation.EsDeGamelists;

        // "~" is the home folder of the user Gamarr runs as, like in ES-DE's own settings
        public static string ExpandHome(string path)
        {
            if (path.IsNullOrWhiteSpace())
            {
                return path;
            }

            path = path.Trim();

            if (path == "~" || path.StartsWith("~/") || path.StartsWith("~\\"))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);

                return path.Length <= 2 ? home : Path.Combine(home, path.Substring(2));
            }

            return path;
        }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
