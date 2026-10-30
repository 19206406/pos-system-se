namespace Identity.Domain.Entities
{
    /// <summary>
    /// A side effect (an email, for now) recorded in the same transaction as the change that causes it,
    /// and delivered later by a background processor. Payloads never contain tokens or passwords:
    /// the processor generates whatever secret it needs at delivery time.
    /// </summary>
    public class OutboxMessage
    {
        public const int TypeMaxLength = 100;
        public const int LastErrorMaxLength = 500;

        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTime OccurredAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public int Attempts { get; set; }
        public string? LastError { get; set; }

        /// <summary> While in the future, one processor instance owns the message and the others skip it. </summary>
        public DateTime? LockedUntil { get; set; }
    }
}
