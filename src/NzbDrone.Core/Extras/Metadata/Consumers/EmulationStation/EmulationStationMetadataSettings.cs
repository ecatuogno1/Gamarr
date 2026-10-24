using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.EmulationStation
{
    public class EmulationStationSettingsValidator : AbstractValidator<EmulationStationMetadataSettings>
    {
    }

    public class EmulationStationMetadataSettings : IProviderConfig
    {
        private static readonly EmulationStationSettingsValidator Validator = new();

        public EmulationStationMetadataSettings()
        {
            Gamelist = true;
            GameImages = true;
        }

        [FieldDefinition(0, Label = "MetadataSettingsEmulationStationGamelist", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataSettingsEmulationStationGamelistHelpText")]
        public bool Gamelist { get; set; }

        [FieldDefinition(1, Label = "MetadataSettingsGameImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "MetadataSettingsEmulationStationImagesHelpText")]
        public bool GameImages { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
