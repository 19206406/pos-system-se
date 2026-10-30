using FluentValidation;

namespace Identity.Application.Features.Auth.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("The refresh token cannot be empty.")
                .MaximumLength(200).WithMessage("The refresh token is not valid.");
        }
    }
}
