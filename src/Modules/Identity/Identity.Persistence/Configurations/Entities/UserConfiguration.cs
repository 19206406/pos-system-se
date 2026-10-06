using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Configurations.Entities
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id).HasName("pk_users");

            builder.Property(u => u.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            builder.Property(u => u.FullName)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("full_name");

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");

            builder.Property(u => u.JobTitle)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("job_title");

            builder.Property(u => u.Email)
                .HasMaxLength(150)
                .IsRequired()
                .HasColumnName("email");

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            builder.Property(u => u.HashPassword)
                .HasColumnName("hash_password");

            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at");



            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("uq_users_email"); 
        }
    }
}
