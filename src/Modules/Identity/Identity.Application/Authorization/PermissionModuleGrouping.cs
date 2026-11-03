using Identity.Application.Dtos.Permissions;

namespace Identity.Application.Authorization
{
    /// <summary> Groups permissions by module, ordered by module name and then by code, for a stable output. </summary>
    public static class PermissionModuleGrouping
    {
        public static IReadOnlyList<PermissionModuleDto> Group(IEnumerable<PermissionDto> permissions, PermissionCatalog catalog) =>
            permissions
                .GroupBy(p => p.Module)
                .Select(group => new PermissionModuleDto(
                    group.Key,
                    catalog.GetModuleName(group.Key),
                    group.OrderBy(p => p.Code, StringComparer.Ordinal)
                        .Select(p => new PermissionItemDto(p.Code, p.Name, p.Description))
                        .ToList()))
                .OrderBy(m => m.ModuleName, StringComparer.Ordinal)
                .ThenBy(m => m.ModuleCode, StringComparer.Ordinal)
                .ToList();
    }
}
