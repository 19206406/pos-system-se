namespace Shared.Authorization
{
    /// <summary>
    /// The permissions a module declares. Each module implements it once and registers it in DI as a singleton;
    /// Identity collects every registration to create the authorization policies and to keep the permissions
    /// table in sync. No module needs a reference to another module to declare its permissions.
    /// </summary>
    public interface IModulePermissionCatalog
    {
        /// <summary> Stable code stored in permissions.module, e.g. "identity". Never rename it once published. </summary>
        string ModuleCode { get; }

        /// <summary> Display name used to group the permissions when building roles. </summary>
        string ModuleName { get; }

        IReadOnlyList<PermissionDefinition> Permissions { get; }
    }
}
