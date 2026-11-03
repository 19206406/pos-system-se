namespace Identity.Application.Features.Users.Queries.GetUserRoles
{
    public sealed record GetUserRolesResponse(Guid UserId, IReadOnlyList<GetUserRolesItem> Roles);

    public sealed record GetUserRolesItem(Guid Id, string RoleName);
}
