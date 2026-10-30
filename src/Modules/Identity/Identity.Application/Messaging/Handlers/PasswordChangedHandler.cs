using Identity.Application.Contracts.Messaging;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Dtos.Authentication;
using Identity.Application.Options;
using Identity.Application.Messaging.Messages;
using Identity.Domain.Entities;

namespace Identity.Application.Messaging.Handlers
{
    public sealed class PasswordChangedHandler : OutboxMessageHandler<PasswordChanged>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;

        public PasswordChangedHandler(IUserRepository userRepository, IEmailSender emailSender)
        {
            _userRepository = userRepository;
            _emailSender = emailSender;
        }

        protected override async Task HandleAsync(PasswordChanged message, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(message.UserId, cancellationToken);
            if (user is null)
                return;

            await _emailSender.SendAsync(new EmailMessage(
                user.Email, user.FullName,
                "Tu contraseña de MiPOS cambió",
                $"Hola {user.FullName}:\n\nLa contraseña de tu cuenta se cambió y se cerraron tus sesiones abiertas.\n\nSi no fuiste tú, avisa de inmediato al administrador."),
                cancellationToken);
        }
    }
}
