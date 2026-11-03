namespace Identity.Presentation.Requests.Users
{
    /// <param name="RoleIds"> The complete set of roles the user must end up with; empty revokes every role. </param>
    public sealed record UpdateUserRolesRequest(IReadOnlyList<Guid> RoleIds);
}
