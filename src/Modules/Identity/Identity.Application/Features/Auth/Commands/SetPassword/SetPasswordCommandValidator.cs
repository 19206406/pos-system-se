using FluentValidation;
using Identity.Application.Validation;

namespace Identity.Application.Features.Auth.Commands.SetPassword
{
    public sealed class SetPasswordCommandValidator : AbstractValidator<SetPasswordCommand>
    {
        public SetPasswordCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("The token cannot be empty.")
                .MaximumLength(255).WithMessage("The token cannot exceed 255 characters.");

            RuleFor(x => x.NewPassword).MustBeValidNewPassword();
        }
    }
}
