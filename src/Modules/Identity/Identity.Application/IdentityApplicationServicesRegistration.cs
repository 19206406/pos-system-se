using Identity.Application.Authorization;
using Identity.Application.Contracts.Services;
using Identity.Application.Features.Users.Commands.CreateUser;
using Identity.Application.Messaging;
using Identity.Application.Messaging.Handlers;
using Identity.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Shared.Authorization;
using Shared.Mediator;

namespace Identity.Application
{
    public static class IdentityApplicationServicesRegistration
    {
        public static IServiceCollection AddIdentityApplicationServices(this IServiceCollection services)
        {

            /// <sumary>
            /// this line is only responsible for obtaining the assembly
            /// nothing else should be added for fluentvalidation validations
            /// </sumary>
            var assembly = typeof(CreateUserCommand).Assembly;

            services.AddMediator(assembly);
            services.AddMediatorValidation(assembly);

            services.AddScoped<IAuthSessionService, AuthSessionService>();
            services.AddScoped<AdminBootstrapper>();

            // Permissions: each module registers its own IModulePermissionCatalog in its registration;
            // PermissionCatalog combines all of them for the authorization policies and the synchronization
            services.AddSingleton<IModulePermissionCatalog, IdentityPermissionCatalog>();
            services.AddSingleton<PermissionCatalog>();
            services.AddScoped<PermissionCatalogSynchronizer>();

            // Messaging: one explicit handler registration per message type; NotificationMessageConsumer finds them by MessageType.
            // The subscription tells the transport which queue to create and what to run for each message.
            services.AddScoped<NotificationMessageConsumer>();
            services.AddSingleton(IdentityMessaging.NotificationsSubscription());
            services.AddScoped<PasswordLinkEmailSender>();
            services.AddScoped<IOutboxMessageHandler, UserInviteRequestedHandler>();
            services.AddScoped<IOutboxMessageHandler, PasswordResetRequestedHandler>();
            services.AddScoped<IOutboxMessageHandler, PasswordChangedHandler>();

            return services;
        }
    }
}
