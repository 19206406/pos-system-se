using Shared.CQRS;

namespace Identity.Application.Features.Roles.Commands.CreateRole
{
    public sealed record CreateRoleCommand(string RoleName, string RoleDescription) : ICommand<CreateRoleResponse>; 
}
