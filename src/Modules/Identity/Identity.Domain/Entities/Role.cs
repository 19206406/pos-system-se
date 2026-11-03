namespace Identity.Domain.Entities
{
    public class Role
    {
        public const int NameMaxLength = 50;
        public const int DescriptionMaxLength = 250;

        public Guid Id { get; set; }
        public string RoleName { get; set; } = null!;
        public string RoleDescription { get; set; } = null!;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        /// <summary>
        /// Leaves the role with exactly <paramref name="permissionIds"/>: adds the missing ones and removes the rest.
        /// Requires <see cref="RolePermissions"/> to be loaded.
        /// </summary>
        public void ReplacePermissions(IReadOnlyCollection<Guid> permissionIds)
        {
            var wanted = permissionIds.ToHashSet();

            foreach (var current in RolePermissions.Where(rp => !wanted.Contains(rp.PermissionId)).ToList())
                RolePermissions.Remove(current);

            var kept = RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

            foreach (var permissionId in wanted.Where(id => !kept.Contains(id)))
                RolePermissions.Add(new RolePermission { RoleId = Id, PermissionId = permissionId });
        }
    }
}
