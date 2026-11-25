using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Configurations.Entities
{
    public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.HasKey(im => new { im.MessageId, im.Handler })
                .HasName("pk_inbox_messages");

            builder.Property(im => im.MessageId)
                .IsRequired()
                .HasColumnName("message_id");

            builder.Property(im => im.Handler)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("handler");

            builder.Property(im => im.ProcessedAt)
                .IsRequired()
                .HasColumnName("processed_at");

            builder.HasIndex(im => im.ProcessedAt)
                .HasDatabaseName("idx_inbox_messages_processed_at"); 
        }
    }
}
