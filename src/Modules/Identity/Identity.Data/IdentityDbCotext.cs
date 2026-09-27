using Identity.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Data
{
    public class IdentityDbCotext : DbContext
    {
        public IdentityDbCotext(DbContextOptions<IdentityDbCotext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UsersRoles { get; set; }
        public DbSet<RolePermission> RolesPermissions { get; set; }
        public DbSet<PasswordToken> passwordTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbCotext).Assembly); 
            base.OnModelCreating(modelBuilder);
        }

    }
}
