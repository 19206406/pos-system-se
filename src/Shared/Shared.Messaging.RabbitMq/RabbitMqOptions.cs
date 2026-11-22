using System.ComponentModel.DataAnnotations;

namespace Shared.Messaging.RabbitMq
{
    /// <summary> Connection and delivery settings. UserName and Password come from user-secrets or environment variables, never from appsettings. </summary>
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "Messaging:RabbitMq";

        [Required]
        public string HostName { get; init; } = "localhost";

        [Range(1, 65535)]
        public int Port { get; init; } = 5672;

        [Required]
        public string UserName { get; init; } = "";

        [Required]
        public string Password { get; init; } = "";

        [Required]
        public string VirtualHost { get; init; } = "/";

        /// <summary> Unacknowledged messages the broker sends to a consumer at once. </summary>
        [Range(1, 1000)]
        public int PrefetchCount { get; init; } = 10;

        /// <summary> Deliveries (the first one included) before a failing message goes to the dead-letter queue. </summary>
        [Range(1, 50)]
        public int MaxDeliveryAttempts { get; init; } = 5;

        /// <summary> How long a failed message waits in the retry queue before it is delivered again. </summary>
        [Range(1, 3600)]
        public int RetryDelaySeconds { get; init; } = 30;
    }
}
