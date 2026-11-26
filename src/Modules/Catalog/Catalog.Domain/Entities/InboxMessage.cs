namespace Catalog.Domain.Entities
{
    public class InboxMessage
    {
        public const int HandlerMaxLength = 200;

        public Guid MessageId { get; set; }
        public string Handler { get; set; } = null!;
        public DateTime ProcessedAt { get; set; }
    }
}
