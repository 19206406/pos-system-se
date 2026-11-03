using FluentValidation;

namespace Identity.Application.Features.Roles.Commands.CreateRole
{
    public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("The name of the new role cannot be left blank.")
                .MaximumLength(50).WithMessage("The name of the new role cannot exceed 100 characters.");

            RuleFor(x => x.RoleDescription)
                .NotEmpty().WithMessage("The description of the new role cannot be left blank.")
                .MaximumLength(250).WithMessage("The description of the new role cannot exceed 100 characters."); 
        }
    }
}
