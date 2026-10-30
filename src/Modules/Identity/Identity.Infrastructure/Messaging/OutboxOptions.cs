using System.ComponentModel.DataAnnotations;

namespace Identity.Infrastructure.Messaging
{
    public sealed class OutboxOptions
    {
        public const string SectionName = "Outbox";

        [Range(1, 300)]
        public int PollingIntervalSeconds { get; init; } = 5;

        [Range(1, 500)]
        public int BatchSize { get; init; } = 20;

        /// <summary> After this many failed attempts the message stays undelivered and is kept for inspection. </summary>
        [Range(1, 50)]
        public int MaxAttempts { get; init; } = 5;

        /// <summary> How long one processor instance owns a claimed message before another may take it. </summary>
        [Range(10, 3600)]
        public int LockSeconds { get; init; } = 60;
    }
}
