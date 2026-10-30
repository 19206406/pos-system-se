namespace Identity.Application.Messaging
{
    /// <summary>
    /// A message stored in the outbox. <see cref="MessageType"/> is the stable name persisted in outbox_messages.type
    /// and used to find its handler; renaming the C# type must not change it.
    /// </summary>
    public interface IOutboxMessage
    {
        static abstract string MessageType { get; }
    }
}
