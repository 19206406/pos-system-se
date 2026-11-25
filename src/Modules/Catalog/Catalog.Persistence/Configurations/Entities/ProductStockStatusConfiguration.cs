using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Configurations.Entities
{
    public class ProductStockStatusConfiguration : IEntityTypeConfiguration<ProductStockStatus>
    {
        public void Configure(EntityTypeBuilder<ProductStockStatus> builder)
        {
            builder.ToTable("product_stock_status");

            builder.HasKey(ps => ps.ProductId)
                .HasName("pk_product_stock_status");

            builder.Property(ps => ps.ProductId)
                .HasColumnName("product_id");

            builder.Property(ps => ps.IsAvailable)
                .IsRequired()
                .HasColumnName("is_available");

            builder.Property(ps => ps.Reason)
                .HasMaxLength(250)
                .HasColumnName("reason");

            builder.Property(ps => ps.SourceOcurredAt)
                .HasColumnName("source_ocurred_at");

            builder.Property(ps => ps.UpdatedAt)
                .HasColumnName("updated_at");

            builder.HasOne(ps => ps.Product)
                .WithOne(p => p.ProductStockStatus)
                .HasForeignKey<ProductStockStatus>(ps => ps.ProductId)
                .HasConstraintName("fk_product_stock_status_product")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
