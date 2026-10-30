namespace Identity.Application.Contracts.Messaging
{
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
    }

    /// <summary> Plain-text email. The body may contain a one-time link: implementations must never log it. </summary>
    public sealed record EmailMessage(string ToAddress, string ToName, string Subject, string Body);
}
