using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shared.Messaging.RabbitMq
{
    public static class MessagingServiceCollectionExtensions
    {
        public const string TransportKey = "Messaging:Transport";

        /// <summary>
        /// Registers the message transport chosen in <c>Messaging:Transport</c>: <c>RabbitMq</c> (default) or <c>InMemory</c>.
        /// Modules add their own <see cref="MessageSubscription"/> registrations; the order of the calls does not matter.
        /// </summary>
        public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            var transport = configuration[TransportKey] ?? MessagingTransport.RabbitMq;

            switch (transport)
            {
                case MessagingTransport.InMemory:
                    services.AddSingleton<IMessagePublisher, InMemoryMessagePublisher>();
                    break;

                case MessagingTransport.RabbitMq:
                    // An invalid or missing setting (for instance the password) stops the application at startup
                    services.AddOptions<RabbitMqOptions>()
                        .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
                        .ValidateDataAnnotations()
                        .ValidateOnStart();

                    services.AddSingleton<RabbitMqConnectionProvider>();
                    services.AddSingleton<RabbitMqTopology>();
                    services.AddSingleton<RabbitMqMessagePublisher>();
                    services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqMessagePublisher>());
                    services.AddHostedService<RabbitMqConsumerService>();

                    // Degraded, not unhealthy: with the broker down the API still works and the outbox keeps the messages
                    services.AddHealthChecks().Add(new HealthCheckRegistration(
                        "rabbitmq",
                        sp => new RabbitMqHealthCheck(sp.GetRequiredService<RabbitMqConnectionProvider>()),
                        HealthStatus.Degraded,
                        tags: null));
                    break;

                default:
                    throw new InvalidOperationException(
                        $"'{TransportKey}' must be '{MessagingTransport.RabbitMq}' or '{MessagingTransport.InMemory}', not '{transport}'.");
            }

            return services;
        }
    }
}
