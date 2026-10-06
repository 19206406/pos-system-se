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
                .HasMaxLength(50)
                .HasColumnName("permission_name");

            builder.Property(p => p.PermissionDescription)
                .HasMaxLength(250)
                .IsRequired()
                .HasColumnName("permission_description");

            builder.Property(p => p.Identifier)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("identifier");

            builder.HasIndex(p => p.Identifier)
                .IsUnique()
                .HasDatabaseName("uq_permissions_identifier");
        }
    }
}
