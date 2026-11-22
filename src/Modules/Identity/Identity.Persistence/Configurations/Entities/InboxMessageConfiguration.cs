using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Configurations.Entities
{
    public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.ToTable("inbox_messages");

            builder.HasKey(m => new { m.MessageId, m.Handler }).HasName("pk_inbox_messages");

            builder.Property(m => m.MessageId)
                .HasColumnName("message_id");

            builder.Property(m => m.Handler)
                .HasMaxLength(InboxMessage.HandlerMaxLength)
                .IsRequired()
                .HasColumnName("handler");

            builder.Property(m => m.ProcessedAt)
                .HasColumnName("processed_at");

            // The retention job deletes by age
            builder.HasIndex(m => m.ProcessedAt)
                .HasDatabaseName("idx_inbox_messages_processed_at");
        }
    }
}
