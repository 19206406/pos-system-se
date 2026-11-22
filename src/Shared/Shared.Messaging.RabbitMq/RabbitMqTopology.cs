using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Shared.Messaging.RabbitMq
{
    /// <summary>
    /// Declares exchanges, queues and bindings from the registered subscriptions. Declarations are idempotent, so publishers
    /// and consumers both call <see cref="EnsureDeclaredAsync"/>: whoever reaches the broker first creates the queues, and a
    /// message published before any consumer started is never dropped for lack of a bound queue.
    ///
    /// Per subscription: the queue itself, a retry queue (TTL, then back to the queue through the default exchange)
    /// and a dead-letter queue. Changing RetryDelaySeconds changes the retry queue arguments, and the broker refuses
    /// to redeclare it: delete the retry queues first.
    /// </summary>
    internal sealed class RabbitMqTopology
    {
        public static string RetryQueue(string queue) => $"{queue}.retry";
        public static string DeadLetterQueue(string queue) => $"{queue}.dlq";

        private readonly RabbitMqConnectionProvider _connections;
        private readonly RabbitMqOptions _options;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly HashSet<string> _declaredExchanges = [];
        private volatile bool _subscriptionsDeclared;

        public RabbitMqTopology(
            RabbitMqConnectionProvider connections, IEnumerable<MessageSubscription> subscriptions, IOptions<RabbitMqOptions> options)
        {
            _connections = connections;
            Subscriptions = subscriptions.ToList();
            _options = options.Value;
        }

        public IReadOnlyList<MessageSubscription> Subscriptions { get; }

        public async Task EnsureDeclaredAsync(string? exchange, CancellationToken cancellationToken)
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                var exchangePending = exchange is not null && !_declaredExchanges.Contains(exchange);
                if (_subscriptionsDeclared && !exchangePending)
                    return;

                var connection = await _connections.GetConnectionAsync(cancellationToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                if (exchangePending)
                    await DeclareExchangeAsync(channel, exchange!, cancellationToken);

                if (!_subscriptionsDeclared)
                {
                    foreach (var subscription in Subscriptions)
                        await DeclareSubscriptionAsync(channel, subscription, cancellationToken);

                    _subscriptionsDeclared = true;
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        private async Task DeclareExchangeAsync(IChannel channel, string exchange, CancellationToken cancellationToken)
        {
            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: cancellationToken);
            _declaredExchanges.Add(exchange);
        }

        private async Task DeclareSubscriptionAsync(IChannel channel, MessageSubscription subscription, CancellationToken cancellationToken)
        {
            if (!_declaredExchanges.Contains(subscription.Exchange))
                await DeclareExchangeAsync(channel, subscription.Exchange, cancellationToken);

            await channel.QueueDeclareAsync(subscription.Queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                DeadLetterQueue(subscription.Queue), durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                RetryQueue(subscription.Queue), durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-message-ttl"] = _options.RetryDelaySeconds * 1000,
                    ["x-dead-letter-exchange"] = "",
                    ["x-dead-letter-routing-key"] = subscription.Queue
                },
                cancellationToken: cancellationToken);

            foreach (var routingKey in subscription.RoutingKeys)
                await channel.QueueBindAsync(subscription.Queue, subscription.Exchange, routingKey, cancellationToken: cancellationToken);
        }
    }
}
