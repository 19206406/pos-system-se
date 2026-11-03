namespace Identity.Application.Dtos.Permissions
{
    /// <summary> Permissions of one module, the shape used to show and pick permissions when building a role. </summary>
    public sealed record PermissionModuleDto(string ModuleCode, string ModuleName, IReadOnlyList<PermissionItemDto> Permissions);

    public sealed record PermissionItemDto(string Code, string Name, string Description);
}
