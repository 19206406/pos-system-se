namespace Identity.Domain.Entities
{
    /// <summary>
    /// Records that a handler already processed a message, so a redelivery (the broker delivers at least once) has no second effect.
    /// The key is the outbox message id plus the handler that processed it.
    /// </summary>
    public class InboxMessage
    {
        public const int HandlerMaxLength = 200;

        public Guid MessageId { get; set; }
        public string Handler { get; set; } = null!;
        public DateTime ProcessedAt { get; set; }
    }
}
