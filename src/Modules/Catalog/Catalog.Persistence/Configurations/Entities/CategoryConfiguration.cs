using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Configurations.Entities
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("categories", t =>
                t.HasCheckConstraint("chk_categories_name_not_blank", "btrim(name) <> ''"));

            builder.HasKey(c => c.Id).HasName("pk_categories");

            builder.Property(c => c.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");

            builder.Property(c => c.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("display_order");

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            builder.Property(c => c.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(c => c.UpdatedAt)
                .HasColumnName("updated_at");


            builder.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("uq_categories_name"); 
        }
    }
}
