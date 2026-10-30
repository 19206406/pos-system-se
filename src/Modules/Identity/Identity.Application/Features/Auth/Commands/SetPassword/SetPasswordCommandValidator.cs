using FluentValidation;
using Identity.Application.Validation;

namespace Identity.Application.Features.Auth.Commands.SetPassword
{
    public sealed class SetPasswordCommandValidator : AbstractValidator<SetPasswordCommand>
    {
        public SetPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("The email address cannot be empty.")
                .EmailAddress().WithMessage("The value you entered is not a valid email address.")
                .MaximumLength(150).WithMessage("The email cannot exceed 150 characters.");

            RuleFor(x => x.Password).MustBeValidNewPassword();

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("The token cannot be empty.")
                .MaximumLength(255).WithMessage("The token cannot exceed 250 characters.");
        }
    }
}
