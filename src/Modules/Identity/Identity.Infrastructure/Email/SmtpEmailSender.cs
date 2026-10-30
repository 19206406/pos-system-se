using Identity.Application.Contracts.Messaging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Identity.Infrastructure.Email
{
    /// <summary> Sends plain-text email over SMTP. The body may hold a one-time link, so nothing here logs it. </summary>
    internal sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;

        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            // SmtpClient is not thread-safe: one per message
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrEmpty(_options.UserName))
                client.Credentials = new NetworkCredential(_options.UserName, _options.Password);

            using var mail = new MailMessage(
                new MailAddress(_options.FromAddress, _options.FromName),
                new MailAddress(message.ToAddress, message.ToName))
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = false,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8
            };

            await client.SendMailAsync(mail, cancellationToken);
        }
    }
}
