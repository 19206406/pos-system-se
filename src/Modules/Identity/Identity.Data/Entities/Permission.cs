namespace Identity.Data.Entities
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string PermissionName { get; set; } = null!; 
        public string PermissionDescription { get; set; } = null!;
        public string Identifier { get; set; } = null!;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>(); 
    }
}
