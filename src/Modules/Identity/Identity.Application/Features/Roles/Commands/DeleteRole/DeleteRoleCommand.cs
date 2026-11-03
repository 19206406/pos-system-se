using Shared.CQRS;

namespace Identity.Application.Features.Roles.Commands.DeleteRole
{
    public sealed record DeleteRoleCommand(Guid RoleId) : ICommand;
}
