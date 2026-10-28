using FluentValidation;

namespace Identity.Application.Features.Auth.Commands.ChangePassword
{
    internal class ChangePasswordValidation : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordValidation()
        {

            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.CurrentSessionId).NotEmpty();

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("La contraseña actual es obligatoria")
                .MaximumLength(25); 

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("La contraseña nueva es obligatoria")
                .MaximumLength(25);

            RuleFor(x => x.NewPassword)
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("La nueva contraseña debe ser diferente a la actual.");
        }
    }
}
