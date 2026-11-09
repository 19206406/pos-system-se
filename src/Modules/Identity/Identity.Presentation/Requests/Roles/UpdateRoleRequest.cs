namespace Identity.Presentation.Requests.Roles
{
    /// <param name="RoleName"> New name of the role; must not belong to another role. </param>
    /// <param name="RoleDescription"> New description of the role. </param>
    /// <param name="PermissionCodes"> The complete set of permission codes the role must end up with. </param>
    public sealed record UpdateRoleRequest(string RoleName, string RoleDescription, IReadOnlyList<string> PermissionCodes);
}
