using FluentValidation;
using Identity.Application.Validation;

namespace Identity.Application.Features.Roles.Commands.UpdateRole
{
    public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(x => x.RoleId).NotEmpty();
            RuleFor(x => x.RoleName).MustBeValidRoleName();
            RuleFor(x => x.RoleDescription).MustBeValidRoleDescription();
            RuleFor(x => x.PermissionCodes).MustBeValidPermissionSelection();
        }
    }
}
