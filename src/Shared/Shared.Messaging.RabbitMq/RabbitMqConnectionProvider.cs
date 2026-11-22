using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Shared.Messaging.RabbitMq
{
    /// <summary>
    /// One connection per process, created on first use. It never connects at startup, so a broker that is down
    /// does not stop the API: publishing fails (the outbox keeps the message) and the consumers retry in the background.
    /// Automatic recovery reconnects and restores consumers when the broker comes back.
    /// </summary>
    internal sealed class RabbitMqConnectionProvider : IAsyncDisposable
    {
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqConnectionProvider> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private IConnection? _connection;

        public RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnectionProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            // A connection that is closed but recovering is kept: replacing it would leak its consumers
            if (_connection is not null)
                return _connection;

            await _gate.WaitAsync(cancellationToken);
            try
            {
                if (_connection is not null)
                    return _connection;

                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    Port = _options.Port,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost,
                    ClientProvidedName = "pos-system",
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true
                };

                _connection = await factory.CreateConnectionAsync(cancellationToken);
                _logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}", _options.HostName, _options.Port);
                return _connection;
            }
            finally
            {
                _gate.Release();
            }
        }

        public bool IsOpen => _connection is { IsOpen: true };

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();

            _gate.Dispose();
        }
    }
}
