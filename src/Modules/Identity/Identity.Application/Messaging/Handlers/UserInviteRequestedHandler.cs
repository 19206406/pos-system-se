using Identity.Application.Contracts.Messaging;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Dtos.Authentication;
using Identity.Application.Options;
using Identity.Application.Messaging.Messages;
using Identity.Domain.Entities;

namespace Identity.Application.Messaging.Handlers
{
    public sealed class UserInviteRequestedHandler : OutboxMessageHandler<UserInviteRequested>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordTokenIssuer _passwordTokenIssuer;
        private readonly PasswordLinkEmailSender _linkEmailSender;

        public UserInviteRequestedHandler(
            IUserRepository userRepository, IPasswordTokenIssuer passwordTokenIssuer, PasswordLinkEmailSender linkEmailSender)
        {
            _userRepository = userRepository;
            _passwordTokenIssuer = passwordTokenIssuer;
            _linkEmailSender = linkEmailSender;
        }

        protected override async Task HandleAsync(UserInviteRequested message, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(message.UserId, cancellationToken);

            // Nothing to send if the account disappeared, was deactivated or already set its password
            if (user is null || !user.IsActive || user.HashPassword is not null)
                return;

            await _linkEmailSender.SendAsync(
                user, _passwordTokenIssuer.IssueInvite,
                "Invitación a MiPOS",
                link => $"Hola {user.FullName}:\n\nSe creó tu cuenta en MiPOS. Define tu contraseña en este enlace:\n\n{link}\n\nSi no esperabas este correo, ignóralo.",
                cancellationToken);
        }
    }
}
