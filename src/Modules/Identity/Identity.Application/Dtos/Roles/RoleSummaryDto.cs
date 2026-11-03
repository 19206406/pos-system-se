namespace Identity.Application.Dtos.Roles
{
    public sealed record RoleSummaryDto(Guid Id, string RoleName, string RoleDescription, int PermissionCount, int UserCount);
}
