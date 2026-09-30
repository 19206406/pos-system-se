using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Configurations.Entities
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles", "identity");

            builder.HasKey(r => r.Id).HasName("pk_roles");

            builder.Property(r => r.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            builder.Property(r => r.RoleName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("role_name");

            builder.Property(r => r.RoleDescription)
                .HasMaxLength(250)
                .IsRequired()
                .HasColumnName("role_description");

            builder.HasIndex(r => r.RoleName)
                .IsUnique()
                .HasDatabaseName("uq_roles_role_name");
        }
    }
}
