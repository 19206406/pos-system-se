using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Configurations.Entities
{
    internal class MenuConfiguration : IEntityTypeConfiguration<Menu>
    {
        public void Configure(EntityTypeBuilder<Menu> builder)
        {
            builder.ToTable("menus", m =>
                m.HasCheckConstraint("chk_menus_name_not_blank", "btrim(name) <> ''"));

            builder.HasKey(m => m.Id)
                .HasName("pk_menus");

            builder.Property(m => m.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");

            builder.Property(m => m.Description)
                .HasMaxLength(250)
                .HasColumnName("description");

            builder.Property(m => m.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            builder.Property(m => m.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(m => m.UpdatedAt)
                .HasColumnName("updated_at");


            builder.HasIndex(m => m.Name)
                .IsUnique()
                .HasDatabaseName("uq_menus_name"); 
        }
    }
}
