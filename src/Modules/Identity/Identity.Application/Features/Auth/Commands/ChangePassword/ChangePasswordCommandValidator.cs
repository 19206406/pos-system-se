using FluentValidation;
using Identity.Application.Validation;

namespace Identity.Application.Features.Auth.Commands.ChangePassword
{
    public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.CurrentSessionId).NotEmpty();

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("The current password cannot be empty.")
                .MaximumLength(PasswordRules.MaxLength);

            RuleFor(x => x.NewPassword).MustBeValidNewPassword();

            RuleFor(x => x.NewPassword)
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("The new password must be different from the current one.");
        }
    }
}
