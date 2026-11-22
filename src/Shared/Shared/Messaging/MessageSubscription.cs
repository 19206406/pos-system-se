namespace Shared.Messaging
{
    /// <summary>
    /// A queue a module reads: which exchange it binds to, which routing keys it wants and what to run for each message.
    /// Each module registers its own subscriptions in DI; the transport creates the topology and calls <see cref="Handle"/>.
    /// </summary>
    /// <param name="Queue"> Durable queue name, <c>&lt;receiver&gt;.&lt;sender&gt;-events</c>. </param>
    /// <param name="Exchange"> Exchange of the emitting module. </param>
    /// <param name="RoutingKeys"> Message types delivered to the queue. </param>
    /// <param name="Handle">
    /// Runs one message inside a DI scope created by the transport. It must be idempotent (inbox) and throw on failure:
    /// the transport then retries later and, after the maximum attempts, moves the message to the dead-letter queue.
    /// </param>
    public sealed record MessageSubscription(
        string Queue,
        string Exchange,
        IReadOnlyList<string> RoutingKeys,
        Func<IServiceProvider, MessageEnvelope, CancellationToken, Task> Handle);
}
