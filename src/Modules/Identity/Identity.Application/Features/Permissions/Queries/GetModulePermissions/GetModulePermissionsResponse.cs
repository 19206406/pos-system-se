using Identity.Application.Dtos.Permissions;

namespace Identity.Application.Features.Permissions.Queries.GetModulePermissions
{
    public sealed record GetModulePermissionsResponse(string ModuleCode, string ModuleName, IReadOnlyList<PermissionItemDto> Permissions);
}
