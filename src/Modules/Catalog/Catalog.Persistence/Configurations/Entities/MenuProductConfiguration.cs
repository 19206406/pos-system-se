using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Configurations.Entities
{
    public class MenuProductConfiguration : IEntityTypeConfiguration<MenuProduct>
    {
        public void Configure(EntityTypeBuilder<MenuProduct> builder)
        {
            builder.ToTable("menu_products");

            builder.HasKey(mp => new { mp.MenuId, mp.ProductId })
                .HasName("pk_menu_products");

            builder.Property(mp => mp.MenuId)
                .HasColumnName("menu_id");

            builder.Property(mp => mp.ProductId)
                .HasColumnName("product_id");

            builder.Property(mp => mp.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");

            // The primary key (menu_id, product_id) already covers lookups by menu
            builder.HasIndex(mp => mp.ProductId)
                .HasDatabaseName("idx_menu_products_product_id");

            builder.HasOne(mp => mp.Menu)
                .WithMany(m => m.MenuProducts)
                .HasForeignKey(mp => mp.MenuId)
                .HasConstraintName("fk_menu_products_menu")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(mp => mp.Product)
                .WithMany(p => p.MenuProducts)
                .HasForeignKey(mp => mp.ProductId)
                .HasConstraintName("fk_menu_products_product")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
