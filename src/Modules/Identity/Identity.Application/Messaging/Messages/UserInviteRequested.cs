namespace Identity.Application.Messaging.Messages
{
    /// <summary> Send an invite link. The token is generated when the message is delivered, not when it is enqueued. </summary>
    public sealed record UserInviteRequested(Guid UserId) : IOutboxMessage
    {
        public static string MessageType => "identity.user-invite-requested";
    }
}
