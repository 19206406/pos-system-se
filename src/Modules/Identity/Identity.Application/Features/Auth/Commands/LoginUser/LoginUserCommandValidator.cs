using FluentValidation;
using Identity.Application.Validation;

namespace Identity.Application.Features.Auth.Commands.LoginUser
{
    public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("The email address cannot be empty.")
                .EmailAddress().WithMessage("The value you entered is not a valid email address.")
                .MaximumLength(150).WithMessage("The email cannot exceed 150 characters.");

            // No minimum here: the new-password rule does not apply to existing passwords.
            // The maximum keeps Argon2id from hashing huge inputs.
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("The password cannot be empty.")
                .MaximumLength(PasswordRules.MaxLength).WithMessage($"The password cannot exceed {PasswordRules.MaxLength} characters.");
        }
    }
}
