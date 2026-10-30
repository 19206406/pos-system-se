using FluentValidation;

namespace Identity.Application.Features.Auth.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenResponse>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("El refresh token es obligatorio.")
                .MaximumLength(200).WithMessage("El refresh token no es válido.");
        }
    }
}
