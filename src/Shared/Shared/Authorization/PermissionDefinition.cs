namespace Shared.Authorization
{
    /// <summary>
    /// One action a module exposes. <paramref name="Code"/> is the unique identifier across every module:
    /// it is stored in the permissions table, travels in the "permission" claim and names the authorization policy.
    /// </summary>
    public sealed record PermissionDefinition(string Code, string Name, string Description);
}
