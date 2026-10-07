using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Configurations.Entities
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("sessions");

            builder.HasKey(s => s.Id).HasName("pk_sessions");

            builder.Property(s => s.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            builder.Property(s => s.UserId)
                .IsRequired()
                .HasColumnName("user_id");

            // refresh token with hash and do not save it in plain text
            builder.Property(s => s.TokenHash)
                .HasMaxLength(255)
                .IsRequired()
                .HasColumnName("token_hash");

            builder.Property(s => s.DeviceInfo)
                .HasMaxLength(100)
                .HasColumnName("device_info");

            builder.Property(s => s.IpAddress)
                .HasMaxLength(50)
                .HasColumnName("ip_address");

            builder.Property(s => s.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(s => s.ExpiresAt)
                .HasColumnName("expires_at");

            builder.Property(s => s.RevokedAt)
                .HasColumnName("revoked_at");

            builder.Property(s => s.ReplacedById)
                .HasColumnName("replaced_by_id"); 


            builder.HasIndex(u => u.TokenHash)
                .IsUnique()
                .HasDatabaseName("uq_sessions_token_hash");

            builder.HasIndex(s => s.UserId)
                .HasDatabaseName("idx_sessions_user_id");

            builder.HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .HasConstraintName("fk_sessions_users")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.ReplacedBy)
                .WithMany()
                .HasForeignKey(s => s.ReplacedById)
                .HasConstraintName("fk_sessions_replaced_by")
                .OnDelete(DeleteBehavior.NoAction); 
        }
    }
}
