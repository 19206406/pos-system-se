using FluentValidation;

namespace Identity.Application.Features.Auth.Queries.ValidatePasswordToken
{
    internal class ValidatePasswordTokenValidation : AbstractValidator<ValidatePasswordTokenQuery>
    {
        public ValidatePasswordTokenValidation()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("The token cannot be empty.")
                .MaximumLength(255).WithMessage("The token cannot exceed 250 characters.");
        }
    }
}

