using FluentValidation;

namespace Identity.Application.Features.Auth.Commands.ValidateToken
{
    internal class ValidateTokenValidation : AbstractValidator<ValidateTokenCommand>
    {
        public ValidateTokenValidation()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("The token cannot be empty.")
                .MaximumLength(255).WithMessage("The token cannot exceed 250 characters.");
        }
    }
}

