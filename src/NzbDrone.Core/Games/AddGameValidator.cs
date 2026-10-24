using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Games
{
    public interface IAddGameValidator
    {
        ValidationResult Validate(Game instance);
    }

    public class AddGameValidator : AbstractValidator<Game>, IAddGameValidator
    {
        public AddGameValidator(RootFolderValidator rootFolderValidator,
                                 RecycleBinValidator recycleBinValidator,
                                 GamePathValidator gamePathValidator,
                                 GameAncestorValidator gameAncestorValidator,
                                 PlatformExistsValidator platformExistsValidator,
                                 IGameService gameService)
        {
            RuleFor(c => c.PlatformId).Cascade(CascadeMode.Stop)
                                      .GreaterThan(0)
                                      .SetValidator(platformExistsValidator);

            RuleFor(c => c.IgdbId).Must((game, igdbId) => !gameService.GameExists(igdbId, game.PlatformId))
                                  .WithMessage("This game has already been added for this platform");

            RuleFor(c => c.Path).Cascade(CascadeMode.Stop)
                                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(recycleBinValidator)
                                .SetValidator(gamePathValidator)
                                .SetValidator(gameAncestorValidator);
        }
    }
}
