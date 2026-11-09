namespace Identity.Presentation.Requests.Roles
{
    /// <param name="RoleName"> Unique name of the role. </param>
    /// <param name="RoleDescription"> What the role is for, shown to whoever assigns it. </param>
    /// <param name="PermissionCodes"> Permission codes the role starts with (e.g. "users.read"); may be empty. </param>
    public sealed record CreateRoleRequest(string RoleName, string RoleDescription, IReadOnlyList<string> PermissionCodes);
}
