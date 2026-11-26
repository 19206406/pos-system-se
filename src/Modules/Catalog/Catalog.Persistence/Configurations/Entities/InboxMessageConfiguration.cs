using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Configurations.Entities
{
    public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.ToTable("inbox_messages");

            builder.HasKey(im => new { im.MessageId, im.Handler })
                .HasName("pk_inbox_messages");

            builder.Property(im => im.MessageId)
                .HasColumnName("message_id");

            builder.Property(im => im.Handler)
                .IsRequired()
                .HasMaxLength(InboxMessage.HandlerMaxLength)
                .HasColumnName("handler");

            builder.Property(im => im.ProcessedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("processed_at");

            // The retention job deletes by age
            builder.HasIndex(im => im.ProcessedAt)
                .HasDatabaseName("idx_inbox_messages_processed_at");
        }
    }
}
