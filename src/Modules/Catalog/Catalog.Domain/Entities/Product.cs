namespace Catalog.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public double Price { get; set; }
        public string TaxType { get; set; } = null!;
        public double TaxRate { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        public Category Category { get; set; } = null!;
        public ProductStockStatus? ProductStockStatus { get; set; }
        public ICollection<MenuProduct> MenuProducts { get; set; } = new List<MenuProduct>();
    }

}
