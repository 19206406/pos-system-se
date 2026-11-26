namespace Catalog.Domain.Entities
{
    public class Product
    {
        public const int CodeMaxLength = 30;
        public const int NameMaxLength = 150;
        public const int DescriptionMaxLength = 500;
        public const int TaxTypeMaxLength = 10;

        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public decimal Price { get; set; }
        public string TaxType { get; set; } = null!;
        public decimal TaxRate { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Category Category { get; set; } = null!;
        public ProductStockStatus? ProductStockStatus { get; set; }
        public ICollection<MenuProduct> MenuProducts { get; set; } = new List<MenuProduct>();
    }
}
