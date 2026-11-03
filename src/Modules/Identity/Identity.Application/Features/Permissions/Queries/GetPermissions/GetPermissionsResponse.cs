using Identity.Application.Dtos.Permissions;

namespace Identity.Application.Features.Permissions.Queries.GetPermissions
{
    public sealed record GetPermissionsResponse(IReadOnlyList<PermissionModuleDto> Modules);
}
