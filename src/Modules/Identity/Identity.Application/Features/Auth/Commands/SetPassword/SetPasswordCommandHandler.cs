using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Messaging;
using Identity.Application.Contracts.Security;
using Identity.Application.Contracts.Services;
using Identity.Application.Messaging.Messages;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.SetPassword
{
    public sealed class SetPasswordCommandHandler : ICommandHandler<SetPasswordCommand>
    {
        // Same message for a missing, used, expired or concurrently consumed token, and for an inactive user
        private const string InvalidTokenMessage = "The link is invalid or has expired.";

        private readonly IUserRepository _userRepository;
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly IAuthSessionService _authSessionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenHasher _tokenHasher;
        private readonly TimeProvider _timeProvider;
        private readonly IOutbox _outbox;

        public SetPasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordTokenRepository passwordTokenRepository,
            IAuthSessionService authSessionService,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ITokenHasher tokenHasher,
            TimeProvider timeProvider,
            IOutbox outbox)
        {
            _userRepository = userRepository;
            _passwordTokenRepository = passwordTokenRepository;
            _authSessionService = authSessionService;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenHasher = tokenHasher;
            _timeProvider = timeProvider;
            _outbox = outbox;
        }

        public async Task<Unit> Handle(SetPasswordCommand command, CancellationToken cancellationToken)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;

            var passwordToken = await _passwordTokenRepository.GetByTokenHashAsync(_tokenHasher.Hash(command.Token), cancellationToken);
            if (passwordToken is null || passwordToken.UsedAt.HasValue || passwordToken.ExpiresAt <= now)
                throw new BadRequestException(InvalidTokenMessage);

            var user = await _userRepository.GetByIdAsync(passwordToken.UserId, cancellationToken);
            if (user is null || !user.IsActive)
                throw new BadRequestException(InvalidTokenMessage);

            // Argon2id is slow: hash before opening the transaction to keep it short
            var newPasswordHash = _passwordHasher.Hash(command.NewPassword);

            await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
            {
                // Conditional update: of two requests with the same token, only one gets here with true
                if (!await _passwordTokenRepository.TryConsumeAsync(passwordToken.Id, now, transactionToken))
                    throw new BadRequestException(InvalidTokenMessage);

                await _passwordTokenRepository.RevokeAllActiveByUserAsync(user.Id, now, transactionToken);

                user.HashPassword = newPasswordHash;
                user.UpdatedAt = now;
                _outbox.Enqueue(new PasswordChanged(user.Id));
                await _unitOfWork.SaveChangesAsync(transactionToken);

                // Always, for invite and reset alike: whoever had the old password loses every session. No auto-login.
                await _authSessionService.RevokeAllUserSessionsAsync(user.Id, transactionToken);
            }, cancellationToken);

            return Unit.Value;
        }
    }
}
