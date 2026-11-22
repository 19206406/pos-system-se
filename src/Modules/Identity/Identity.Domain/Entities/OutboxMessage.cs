namespace Identity.Domain.Entities
{
    /// <summary>
    /// A message (a request to send an email, for now) recorded in the same transaction as the change that causes it,
    /// and published later to the message broker by a background dispatcher. Payloads never contain tokens or passwords:
    /// whoever consumes the message generates the secret it needs at that moment.
    /// </summary>
    public class OutboxMessage
    {
        public const int TypeMaxLength = 100;
        public const int LastErrorMaxLength = 500;

        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTime OccurredAt { get; set; }

        /// <summary> Ties together every message produced by one operation (the request trace id), for tracing across modules. </summary>
        public Guid? CorrelationId { get; set; }

        /// <summary> When the broker confirmed it accepted the message. </summary>
        public DateTime? ProcessedAt { get; set; }
        public int Attempts { get; set; }
        public string? LastError { get; set; }

        /// <summary> While in the future, one processor instance owns the message and the others skip it. </summary>
        public DateTime? LockedUntil { get; set; }
    }
}
