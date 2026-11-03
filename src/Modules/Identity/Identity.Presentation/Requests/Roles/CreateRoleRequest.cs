namespace Identity.Presentation.Requests.Roles
{
    /// <param name="PermissionCodes"> Permission codes the role starts with (e.g. "users.read"); may be empty. </param>
    public sealed record CreateRoleRequest(string RoleName, string RoleDescription, IReadOnlyList<string> PermissionCodes);
}
