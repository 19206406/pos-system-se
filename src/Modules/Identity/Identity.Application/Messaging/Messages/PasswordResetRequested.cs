namespace Identity.Application.Messaging.Messages
{
    /// <summary> Send a reset link. The token is generated when the message is delivered, not when it is enqueued. </summary>
    public sealed record PasswordResetRequested(Guid UserId) : IOutboxMessage
    {
        public static string MessageType => "identity.password-reset-requested";
    }
}
