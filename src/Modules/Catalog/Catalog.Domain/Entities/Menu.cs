namespace Catalog.Domain.Entities
{
    public class Menu
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 255;

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<MenuProduct> MenuProducts { get; set; } = new List<MenuProduct>();
    }
}
