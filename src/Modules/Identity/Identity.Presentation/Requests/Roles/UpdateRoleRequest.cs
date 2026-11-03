namespace Identity.Presentation.Requests.Roles
{
    /// <param name="PermissionCodes"> The complete set of permission codes the role must end up with. </param>
    public sealed record UpdateRoleRequest(string RoleName, string RoleDescription, IReadOnlyList<string> PermissionCodes);
}
