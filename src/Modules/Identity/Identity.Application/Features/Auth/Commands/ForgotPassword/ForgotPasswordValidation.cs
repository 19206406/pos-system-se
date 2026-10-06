using FluentValidation;

namespace Identity.Application.Features.Auth.Commands.ForgotPassword
{
    internal class ForgotPasswordValidation : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordValidation()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("The email address cannot be empty.")
                .EmailAddress().WithMessage("The value you entered is not a valid email address.")
                .MaximumLength(150).WithMessage("The email cannot exceed 150 characters.");
        }
    }
}
