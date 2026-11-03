using FluentValidation;

namespace Identity.Application.Features.Roles.Commands.DeleteRole
{
    public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
    {
        public DeleteRoleCommandValidator() =>
            RuleFor(x => x.RoleId).NotEmpty();
    }
}
