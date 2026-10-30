using Identity.Application.Contracts.Messaging;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Dtos.Authentication;
using Identity.Application.Options;
using Identity.Application.Messaging.Messages;
using Identity.Domain.Entities;

namespace Identity.Application.Messaging.Handlers
{
    /// <summary>
    /// Shared delivery of invite and reset links: invalidate older links, issue a new token, store only its hash,
    /// then email the plain token. Running it twice for the same message just replaces the first link.
    /// </summary>
    public sealed class PasswordLinkEmailSender
    {
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IdentityLinksOptions _links;
        private readonly TimeProvider _timeProvider;

        public PasswordLinkEmailSender(
            IPasswordTokenRepository passwordTokenRepository,
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            IdentityLinksOptions links,
            TimeProvider timeProvider)
        {
            _passwordTokenRepository = passwordTokenRepository;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _links = links;
            _timeProvider = timeProvider;
        }

        public async Task SendAsync(
            User user, Func<DateTime, IssuedPasswordToken> issue,
            string subject, Func<string, string> buildBody, CancellationToken cancellationToken)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;

            // Only the newest link works (decision D9)
            await _passwordTokenRepository.RevokeAllActiveByUserAsync(user.Id, now, cancellationToken);

            var issued = issue(now);
            issued.Entity.UserId = user.Id;
            _passwordTokenRepository.Add(issued.Entity);

            // Saved before sending, so the link already works when the email arrives
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var link = $"{_links.FrontendBaseUrl.TrimEnd('/')}{_links.SetPasswordPath}?token={Uri.EscapeDataString(issued.PlainText)}";
            await _emailSender.SendAsync(new EmailMessage(user.Email, user.FullName, subject, buildBody(link)), cancellationToken);
        }
    }
}
