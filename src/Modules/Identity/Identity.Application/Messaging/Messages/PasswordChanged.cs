namespace Identity.Application.Messaging.Messages
{
    /// <summary> Tell the user their password changed, so an unexpected change is noticed. </summary>
    public sealed record PasswordChanged(Guid UserId) : IOutboxMessage
    {
        public static string MessageType => "identity.password-changed";
    }
}
