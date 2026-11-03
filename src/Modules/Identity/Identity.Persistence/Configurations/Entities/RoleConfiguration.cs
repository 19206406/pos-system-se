using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Persistence.Seed;

namespace Identity.Persistence.Configurations.Entities
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles");

            builder.HasKey(r => r.Id).HasName("pk_roles");

            builder.Property(r => r.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            builder.Property(r => r.RoleName)
                .IsRequired()
                .HasMaxLength(Role.NameMaxLength)
                .HasColumnName("role_name");

            builder.Property(r => r.RoleDescription)
                .HasMaxLength(Role.DescriptionMaxLength)
                .IsRequired()
                .HasColumnName("role_description");

            builder.HasIndex(r => r.RoleName)
                .IsUnique()
                .HasDatabaseName("uq_roles_role_name");

            builder.HasData(IdentitySeedData.Roles);
        }
    }
}
