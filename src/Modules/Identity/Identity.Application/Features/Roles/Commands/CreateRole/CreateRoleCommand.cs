using Shared.CQRS;

namespace Identity.Application.Features.Roles.Commands.CreateRole
{
    /// <param name="PermissionCodes"> Permissions the role starts with; may be empty. Duplicates are ignored. </param>
    public sealed record CreateRoleCommand(string RoleName, string RoleDescription, IReadOnlyList<string> PermissionCodes)
        : ICommand<CreateRoleResponse>;
}
