using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Configurations.Entities
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products", p =>
            {
                p.HasCheckConstraint("chk_products_code_format", "code ~ '^[A-Z0-9][A-Z0-9_-]*$'");
                p.HasCheckConstraint("chk_products_name_not_blank", "btrim(name) <> ''");
                p.HasCheckConstraint("chk_products_price_positive", "price > 0");
                p.HasCheckConstraint("chk_products_tax_type", "tax_type IN ('iva', 'inc', 'exempt', 'excluded')");
                p.HasCheckConstraint("chk_products_tax_rate",
                    "(tax_type IN ('exempt', 'excluded') AND tax_rate = 0) OR (tax_type IN ('iva', 'inc') AND tax_rate > 0 AND tax_rate < 100)");
            });

            builder.HasKey(p => p.Id)
                .HasName("pk_products");

            builder.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            builder.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(Product.CodeMaxLength)
                .HasColumnName("code");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(Product.NameMaxLength)
                .HasColumnName("name");

            builder.Property(p => p.Description)
                .HasMaxLength(Product.DescriptionMaxLength)
                .HasColumnName("description");

            builder.Property(p => p.CategoryId)
                .IsRequired()
                .HasColumnName("category_id");

            builder.Property(p => p.Price)
                .IsRequired()
                .HasPrecision(14, 2)
                .HasColumnName("price");

            builder.Property(p => p.TaxType)
                .IsRequired()
                .HasMaxLength(Product.TaxTypeMaxLength)
                .HasColumnName("tax_type");

            builder.Property(p => p.TaxRate)
                .IsRequired()
                .HasPrecision(5, 2)
                .HasColumnName("tax_rate");

            builder.Property(p => p.IsEnabled)
                .HasDefaultValue(true)
                .HasColumnName("is_enabled");

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            builder.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at");

            builder.HasIndex(p => p.Code)
                .IsUnique()
                .HasDatabaseName("uq_products_code");

            builder.HasIndex(p => p.CategoryId)
                .HasDatabaseName("idx_products_category_id");

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .HasConstraintName("fk_products_category")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
