using FluentValidation.Validators;
using NzbDrone.Core.Games;

namespace NzbDrone.Core.Validation.Paths
{
    // Validates an IgdbId property. The instance being validated must also have a PlatformId property,
    // because a game is unique per (IgdbId, PlatformId), not per IgdbId.
    public class GameExistsValidator : PropertyValidator
    {
        private readonly IGameService _gameService;

        public GameExistsValidator(IGameService gameService)
        {
            _gameService = gameService;
        }

        protected override string GetDefaultMessageTemplate() => "This game has already been added for this platform";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var igdbId = (int)context.PropertyValue;
            dynamic instance = context.InstanceToValidate;
            int platformId = instance.PlatformId;

            return !_gameService.GameExists(igdbId, platformId);
        }
    }
}
