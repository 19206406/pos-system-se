namespace Catalog.Domain.Entities
{
    public class ProductStockStatus
    {
        public const int ReasonMaxLength = 250;

        public Guid ProductId { get; set; }
        public bool IsAvailable { get; set; }
        public string? Reason { get; set; }
        public DateTime SourceOccurredAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Product Product { get; set; } = null!;
    }
}
