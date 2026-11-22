using Identity.Application.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging;

namespace Identity.Application.Messaging
{
    /// <summary> Names of the exchange and queue Identity uses on the message broker. </summary>
    public static class IdentityMessaging
    {
        /// <summary> Exchange where the Identity outbox dispatcher publishes. </summary>
        public const string Exchange = "identity.events";

        /// <summary> Queue read by the notifications worker (the emails Identity sends). </summary>
        public const string NotificationsQueue = "identity.notifications";

        public const int MessageVersion = 1;

        /// <summary> The notifications worker: Identity publishes requests to send an email and consumes them itself. </summary>
        public static MessageSubscription NotificationsSubscription() => new(
            NotificationsQueue,
            Exchange,
            [UserInviteRequested.MessageType, PasswordResetRequested.MessageType, PasswordChanged.MessageType],
            (services, message, cancellationToken) =>
                services.GetRequiredService<NotificationMessageConsumer>().HandleAsync(message, cancellationToken));
    }
}
