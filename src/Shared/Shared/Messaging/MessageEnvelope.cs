namespace Shared.Messaging
{
    /// <summary>
    /// What travels between modules: a small fixed envelope plus the JSON payload. The payload is plain data and ids,
    /// never entities, tokens or passwords. <see cref="MessageId"/> is the outbox row id and the idempotency key of the receivers.
    /// </summary>
    /// <param name="MessageId"> Id of the outbox row that produced the message. </param>
    /// <param name="Type"> Stable message name; it is also the routing key. </param>
    /// <param name="Version"> Payload version. Changes are additive only; an incompatible change creates a new <paramref name="Type"/>. </param>
    /// <param name="OccurredAtUtc"> Instant of the fact, in UTC. </param>
    /// <param name="CorrelationId"> Ties together every message of one operation, for tracing. </param>
    /// <param name="Payload"> JSON text. </param>
    public sealed record MessageEnvelope(
        Guid MessageId,
        string Type,
        int Version,
        DateTime OccurredAtUtc,
        Guid? CorrelationId,
        string Payload);
}
