using Identity.Application.Dtos.Permissions;

namespace Identity.Application.Dtos.Roles
{
    public sealed record RoleDetailDto(Guid Id, string RoleName, string RoleDescription, IReadOnlyList<PermissionDto> Permissions);
}
