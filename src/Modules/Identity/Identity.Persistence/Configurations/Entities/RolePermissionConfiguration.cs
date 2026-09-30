using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Configurations.Entities
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("roles_permissions", "identity");

            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId })
                .HasName("pk_roles_permissions");

            builder.Property(rp => rp.RoleId)
                .IsRequired()
                .HasColumnName("role_id");

            builder.Property(rp => rp.PermissionId)
                .IsRequired()
                .HasColumnName("permission_id");

            builder.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .HasConstraintName("fk_roles_permissions_roles")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rp => rp.Permission)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .HasConstraintName("fk_roles_permissions_permissions")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
