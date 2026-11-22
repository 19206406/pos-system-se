using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shared.Messaging.RabbitMq
{
    /// <summary> Reports whether the connection to the broker is open. It does not open one: the connection is created on first use. </summary>
    internal sealed class RabbitMqHealthCheck : IHealthCheck
    {
        private readonly RabbitMqConnectionProvider _connections;

        public RabbitMqHealthCheck(RabbitMqConnectionProvider connections)
        {
            _connections = connections;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
            Task.FromResult(_connections.IsOpen
                ? HealthCheckResult.Healthy("RabbitMQ connection is open.")
                : new HealthCheckResult(context.Registration.FailureStatus, "RabbitMQ connection is not open."));
    }
}
