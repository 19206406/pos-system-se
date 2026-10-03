using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Configurations.Entities
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("users_roles");

            builder.HasKey(ur => new { ur.UserId, ur.RoleId })
                .HasName("pk_users_roles"); 

            builder.Property(ur => ur.UserId)
                .IsRequired()
                .HasColumnName("user_id");

            builder.Property(ur => ur.RoleId)
                .IsRequired()
                .HasColumnName("role_id");

            builder.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .HasConstraintName("fk_users_roles_users")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .HasConstraintName("fk_users_roles_roles")
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
