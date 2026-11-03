namespace Identity.Application.Dtos.Roles
{
    /// <summary> Just enough of a role to list it next to a user. </summary>
    public sealed record RoleReferenceDto(Guid Id, string RoleName);
}
