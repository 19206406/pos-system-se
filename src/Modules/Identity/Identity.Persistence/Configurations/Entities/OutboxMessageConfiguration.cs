using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Configurations.Entities
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages", t =>
                t.HasCheckConstraint("chk_outbox_messages_attempts", "attempts >= 0"));

            builder.HasKey(m => m.Id).HasName("pk_outbox_messages");

            builder.Property(m => m.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            builder.Property(m => m.Type)
                .HasMaxLength(OutboxMessage.TypeMaxLength)
                .IsRequired()
                .HasColumnName("type");

            builder.Property(m => m.Payload)
                .HasColumnType("jsonb")
                .IsRequired()
                .HasColumnName("payload");

            builder.Property(m => m.OccurredAt)
                .HasColumnName("occurred_at");

            builder.Property(m => m.ProcessedAt)
                .HasColumnName("processed_at");

            builder.Property(m => m.Attempts)
                .HasDefaultValue(0)
                .HasColumnName("attempts");

            builder.Property(m => m.LastError)
                .HasMaxLength(OutboxMessage.LastErrorMaxLength)
                .HasColumnName("last_error");

            builder.Property(m => m.LockedUntil)
                .HasColumnName("locked_until");

            // The processor only ever looks for pending messages, oldest first
            builder.HasIndex(m => m.OccurredAt)
                .HasFilter("processed_at IS NULL")
                .HasDatabaseName("idx_outbox_messages_pending");
        }
    }
}
