namespace Identity.Application.Dtos.Permissions
{
    /// <summary> A row of the permissions table, as read by queries. </summary>
    public sealed record PermissionDto(string Code, string Name, string Description, string Module);
}
