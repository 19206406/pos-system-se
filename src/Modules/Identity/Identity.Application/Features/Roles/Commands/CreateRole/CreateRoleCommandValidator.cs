using FluentValidation;
using Identity.Application.Validation;

namespace Identity.Application.Features.Roles.Commands.CreateRole
{
    public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.RoleName).MustBeValidRoleName();
            RuleFor(x => x.RoleDescription).MustBeValidRoleDescription();
            RuleFor(x => x.PermissionCodes).MustBeValidPermissionSelection();
        }
    }
}
