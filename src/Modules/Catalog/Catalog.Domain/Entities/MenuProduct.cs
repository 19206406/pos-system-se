namespace Catalog.Domain.Entities
{
    public class MenuProduct
    {
        public Guid MenuId { get; set; }
        public Guid ProductId { get; set; }
        public int DisplayOrder { get; set; }

        public Menu Menu { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }

}
