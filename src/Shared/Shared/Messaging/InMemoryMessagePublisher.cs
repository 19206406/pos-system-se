using Microsoft.Extensions.DependencyInjection;

namespace Shared.Messaging
{
    /// <summary>
    /// Transport without a broker. The matching subscriptions run inside <see cref="PublishAsync"/>, each in its own scope,
    /// and a failure propagates to the dispatcher, which keeps the outbox row pending and retries it.
    /// It keeps the same contract as the RabbitMQ transport (handlers must be idempotent) but not its retry or dead-letter queues.
    /// </summary>
    public sealed class InMemoryMessagePublisher : IMessagePublisher
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IReadOnlyList<MessageSubscription> _subscriptions;

        public InMemoryMessagePublisher(IServiceScopeFactory scopeFactory, IEnumerable<MessageSubscription> subscriptions)
        {
            _scopeFactory = scopeFactory;
            _subscriptions = subscriptions.ToList();
        }

        public async Task PublishAsync(string exchange, MessageEnvelope message, CancellationToken cancellationToken)
        {
            foreach (var subscription in _subscriptions)
            {
                if (subscription.Exchange != exchange || !subscription.RoutingKeys.Contains(message.Type))
                    continue;

                using var scope = _scopeFactory.CreateScope();
                await subscription.Handle(scope.ServiceProvider, message, cancellationToken);
            }
        }
    }
}
