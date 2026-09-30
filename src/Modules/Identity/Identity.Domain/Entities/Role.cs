namespace Identity.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = null!;
        public string RoleDescription { get; set; } = null!;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>(); 
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>(); 
    }
}
