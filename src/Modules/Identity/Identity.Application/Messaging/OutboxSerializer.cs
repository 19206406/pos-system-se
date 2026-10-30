using System.Text.Json;

namespace Identity.Application.Messaging
{
    /// <summary> Single JSON format for outbox payloads, shared by the writer (Persistence) and the handlers. </summary>
    public static class OutboxSerializer
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

        public static string Serialize<TMessage>(TMessage message) where TMessage : IOutboxMessage =>
            JsonSerializer.Serialize(message, Options);

        public static TMessage Deserialize<TMessage>(string payload) where TMessage : IOutboxMessage =>
            JsonSerializer.Deserialize<TMessage>(payload, Options)
                ?? throw new InvalidOperationException($"Outbox payload for '{TMessage.MessageType}' is empty.");
    }
}
