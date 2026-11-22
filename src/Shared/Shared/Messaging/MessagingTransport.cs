namespace Shared.Messaging
{
    public static class MessagingTransport
    {
        public const string RabbitMq = "RabbitMq";

        /// <summary> No broker: messages are handed to the subscriptions inside the publishing call. Tests and development without Docker. </summary>
        public const string InMemory = "InMemory";
    }
}
