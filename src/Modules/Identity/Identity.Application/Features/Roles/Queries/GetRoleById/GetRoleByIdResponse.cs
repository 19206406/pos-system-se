using Identity.Application.Dtos.Permissions;

namespace Identity.Application.Features.Roles.Queries.GetRoleById
{
    /// <param name="Modules"> The permissions the role holds, grouped by module; modules without any are omitted. </param>
    public sealed record GetRoleByIdResponse(
        Guid Id, string RoleName, string RoleDescription, bool IsSystemRole, IReadOnlyList<PermissionModuleDto> Modules);
}
