using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Data.Configurations
{
    public class PasswordTokenConfiguration : IEntityTypeConfiguration<PasswordToken>
    {
        public void Configure(EntityTypeBuilder<PasswordToken> builder)
        {
            builder.ToTable("password_tokens", t => 
                t.HasCheckConstraint("chk_password_tokens_token_type", "token_type IN ('invite', 'reset')"));

            builder.HasKey(pt => pt.Id).HasName("pk_password_tokens");

            builder.Property(pt => pt.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            builder.Property(pt => pt.UserId)
                .IsRequired()
                .HasColumnName("user_id");

            builder.Property(pt => pt.TokenHash)
                .HasMaxLength(255)
                .IsRequired()
                .HasColumnName("token_hash");

            builder.Property(pt => pt.TokenType)
                .HasMaxLength(20)
                .HasColumnName("token_type");

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("now()"); 

            builder.Property(u => u.ExpiresAt)
                .HasColumnName("expires_at");

            builder.Property(u => u.UsedAt)
                .HasColumnName("used_at");


            builder.HasIndex(pt => pt.TokenHash)
                .IsUnique()
                .HasDatabaseName("uq_password_tokens_token_hash");

            builder.HasIndex(pt => pt.UserId)
                .HasDatabaseName("idx_password_tokens_user_id");

            builder.HasOne(pt => pt.User)
                .WithMany(u => u.PasswordTokens)
                .HasForeignKey(pt => pt.UserId)
                .HasConstraintName("fk_password_tokens_users")
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
