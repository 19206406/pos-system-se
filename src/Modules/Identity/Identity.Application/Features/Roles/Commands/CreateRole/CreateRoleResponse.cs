namespace Identity.Application.Features.Roles.Commands.CreateRole
{
    public sealed record CreateRoleResponse(Guid Id, string RoleName, string RoleDescription, IReadOnlyList<string> PermissionCodes);
}
