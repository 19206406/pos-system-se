using Shared.CQRS;

namespace Identity.Application.Features.Roles.Commands.UpdateRole
{
    /// <param name="PermissionCodes">
    /// The complete set of permissions the role must end up with: missing ones are granted, absent ones revoked.
    /// </param>
    public sealed record UpdateRoleCommand(Guid RoleId, string RoleName, string RoleDescription, IReadOnlyList<string> PermissionCodes)
        : ICommand;
}
