using Identity.Application.Contracts.Messaging;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Dtos.Authentication;
using Identity.Application.Options;
using Identity.Application.Messaging.Messages;
using Identity.Domain.Entities;

namespace Identity.Application.Messaging.Handlers
{
    public sealed class PasswordResetRequestedHandler : OutboxMessageHandler<PasswordResetRequested>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordTokenIssuer _passwordTokenIssuer;
        private readonly PasswordLinkEmailSender _linkEmailSender;

        public PasswordResetRequestedHandler(
            IUserRepository userRepository, IPasswordTokenIssuer passwordTokenIssuer, PasswordLinkEmailSender linkEmailSender)
        {
            _userRepository = userRepository;
            _passwordTokenIssuer = passwordTokenIssuer;
            _linkEmailSender = linkEmailSender;
        }

        protected override async Task HandleAsync(PasswordResetRequested message, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(message.UserId, cancellationToken);
            if (user is null || !user.IsActive)
                return;

            await _linkEmailSender.SendAsync(
                user, _passwordTokenIssuer.IssueReset,
                "Restablecer tu contraseña de MiPOS",
                link => $"Hola {user.FullName}:\n\nRecibimos una solicitud para restablecer tu contraseña. Usa este enlace:\n\n{link}\n\nSi no lo pediste, ignora este correo: tu contraseña actual sigue funcionando.",
                cancellationToken);
        }
    }
}
