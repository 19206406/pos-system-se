using Identity.Domain.Entities;
using Shared.Authorization;
using System.Text.RegularExpressions;

namespace Identity.Application.Authorization
{
    /// <summary>
    /// Every permission declared by every module (all <see cref="IModulePermissionCatalog"/> registrations), validated
    /// once. The authorization policies and the permission synchronization both read from here, so they cannot drift.
    /// An invalid declaration (duplicate code, bad format, text too long) throws on construction.
    /// </summary>
    public sealed partial class PermissionCatalog
    {
        private readonly Dictionary<string, string> _moduleNamesByCode;

        public PermissionCatalog(IEnumerable<IModulePermissionCatalog> moduleCatalogs)
        {
            Modules = moduleCatalogs.ToList();

            Validate(Modules);

            _moduleNamesByCode = Modules.ToDictionary(m => m.ModuleCode, m => m.ModuleName);
            AllCodes = Modules.SelectMany(m => m.Permissions).Select(p => p.Code).ToList();
        }

        public IReadOnlyList<IModulePermissionCatalog> Modules { get; }

        public IReadOnlyList<string> AllCodes { get; }

        public bool IsDeclaredModule(string moduleCode) => _moduleNamesByCode.ContainsKey(moduleCode);

        /// <summary> Display name of a module; falls back to the code for a module that no longer declares permissions. </summary>
        public string GetModuleName(string moduleCode) =>
            _moduleNamesByCode.TryGetValue(moduleCode, out var name) ? name : moduleCode;

        private static void Validate(IReadOnlyList<IModulePermissionCatalog> modules)
        {
            var errors = new List<string>();

            foreach (var duplicate in modules.GroupBy(m => m.ModuleCode).Where(g => g.Count() > 1))
                errors.Add($"The module code '{duplicate.Key}' is declared by more than one catalog.");

            foreach (var module in modules)
            {
                if (!ModuleCodePattern().IsMatch(module.ModuleCode) || module.ModuleCode.Length > Permission.ModuleMaxLength)
                    errors.Add($"The module code '{module.ModuleCode}' must be lowercase letters, digits or '-', up to {Permission.ModuleMaxLength} characters.");

                if (string.IsNullOrWhiteSpace(module.ModuleName))
                    errors.Add($"The module '{module.ModuleCode}' has no display name.");

                foreach (var permission in module.Permissions)
                {
                    if (!PermissionCodePattern().IsMatch(permission.Code) || permission.Code.Length > Permission.IdentifierMaxLength)
                        errors.Add($"The permission code '{permission.Code}' must look like 'resource.action', up to {Permission.IdentifierMaxLength} characters.");

                    if (string.IsNullOrWhiteSpace(permission.Name) || permission.Name.Length > Permission.NameMaxLength)
                        errors.Add($"The permission '{permission.Code}' needs a name of 1 to {Permission.NameMaxLength} characters.");

                    if (string.IsNullOrWhiteSpace(permission.Description) || permission.Description.Length > Permission.DescriptionMaxLength)
                        errors.Add($"The permission '{permission.Code}' needs a description of 1 to {Permission.DescriptionMaxLength} characters.");
                }
            }

            var duplicateCodes = modules
                .SelectMany(m => m.Permissions.Select(p => (Module: m.ModuleCode, p.Code)))
                .GroupBy(x => x.Code)
                .Where(g => g.Count() > 1);

            foreach (var duplicate in duplicateCodes)
                errors.Add($"The permission code '{duplicate.Key}' is declared more than once (modules: {string.Join(", ", duplicate.Select(x => x.Module))}).");

            if (errors.Count > 0)
                throw new InvalidOperationException("Invalid permission catalog:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
        }

        [GeneratedRegex("^[a-z][a-z0-9-]*$")]
        private static partial Regex ModuleCodePattern();

        // Two or more dot-separated segments: "users.read", "orders.items.cancel"
        [GeneratedRegex(@"^[a-z][a-z0-9-]*(\.[a-z][a-z0-9-]*)+$")]
        private static partial Regex PermissionCodePattern();
    }
}
