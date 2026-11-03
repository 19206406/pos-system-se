namespace Identity.Application.Features.Roles.Queries.GetRoles
{
    public sealed record GetRolesResponse(IReadOnlyList<GetRolesItem> Items);

    /// <param name="IsSystemRole"> True for Admin: it cannot be modified or deleted. </param>
    public sealed record GetRolesItem(
        Guid Id, string RoleName, string RoleDescription, bool IsSystemRole, int PermissionCount, int UserCount);
}
