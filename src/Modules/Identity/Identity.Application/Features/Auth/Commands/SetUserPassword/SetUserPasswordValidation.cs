using FluentValidation;

namespace Identity.Application.Features.Auth.Commands.SetUserPassword
{
    public class SetUserPasswordValidation : AbstractValidator<SetUserPasswordCommand>
    {
        public SetUserPasswordValidation()
        {

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("The email address cannot be empty.")
                .EmailAddress().WithMessage("The value you entered is not a valid email address.")
                .MaximumLength(150).WithMessage("The email cannot exceed 150 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("The password cannot be empty.")
                .MinimumLength(4).WithMessage("The password must be at least 4 characters long.");
        }
    }
}
