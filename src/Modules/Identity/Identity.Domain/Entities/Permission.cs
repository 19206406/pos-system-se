namespace Identity.Domain.Entities
{
    /// <summary>
    /// A predefined action of the application. Rows are not administered through the API: they are created and kept
    /// up to date by the permission synchronization from the catalogs each module declares in code.
    /// </summary>
    public class Permission
    {
        public const int IdentifierMaxLength = 50;
        public const int NameMaxLength = 50;
        public const int DescriptionMaxLength = 250;
        public const int ModuleMaxLength = 50;

        public Guid Id { get; set; }
        public string PermissionName { get; set; } = null!;
        public string PermissionDescription { get; set; } = null!;

        /// <summary> Unique code of the permission (e.g. "users.read"); the value of the "permission" claim. </summary>
        public string Identifier { get; set; } = null!;

        /// <summary> Code of the module that declares the permission (e.g. "identity"). </summary>
        public string Module { get; set; } = null!;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
