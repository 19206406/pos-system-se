using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Configurations.Entities
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("permissions");

            builder.HasKey(p => p.Id).HasName("pk_permissions");

            builder.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            builder.Property(p => p.PermissionName)
                .IsRequired()
                .HasMaxLength(Permission.NameMaxLength)
                .HasColumnName("permission_name");

            builder.Property(p => p.PermissionDescription)
                .HasMaxLength(Permission.DescriptionMaxLength)
                .IsRequired()
                .HasColumnName("permission_description");

            builder.Property(p => p.Identifier)
                .HasMaxLength(Permission.IdentifierMaxLength)
                .IsRequired()
                .HasColumnName("identifier");

            builder.Property(p => p.Module)
                .HasMaxLength(Permission.ModuleMaxLength)
                .IsRequired()
                .HasColumnName("module");

            // Also the conflict target of the upsert in PermissionSynchronizationStore
            builder.HasIndex(p => p.Identifier)
                .IsUnique()
                .HasDatabaseName("uq_permissions_identifier");

            builder.HasIndex(p => p.Module)
                .HasDatabaseName("idx_permissions_module");

            // No HasData: rows come from the module catalogs through PermissionCatalogSynchronizer
        }
    }
}
