namespace Catalog.Domain.Entities
{
    public class ProductStockStatus
    {
        public Guid ProductId { get; set; }
        public bool IsAvailable { get; set; }
        public string? Reason { get; set; }
        public DateTime SourceOcurredAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Product Product { get; set; } = null!;
    }
}
