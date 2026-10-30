using Identity.Application.Contracts.Messaging;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Contracts.Services;
using Identity.Application.Messaging.Messages;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.ChangePassword
{
    public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly TimeProvider _timeProvider;
        private readonly IAuthSessionService _authSessionService;
        private readonly IOutbox _outbox;

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider,
            IPasswordHasher passwordHasher,
            IAuthSessionService authSessionService,
            IOutbox outbox)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _authSessionService = authSessionService;
            _passwordHasher = passwordHasher;
            _timeProvider = timeProvider;
            _outbox = outbox;
        }

        public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
                ?? throw new NotFoundException("user", command.UserId.ToString());

            if (!user.IsActive || user.HashPassword is null)
                throw new BusinessException("The password of this account cannot be changed.");

            // 422, never 401: a 401 makes the client assume the session died and log the user out
            if (!_passwordHasher.Verify(command.CurrentPassword, user.HashPassword))
                throw new BusinessException("The current password is incorrect.");

            // Save first, revoke after: if something fails in between, the worst case is one session too many,
            // never a password that was not changed
            user.HashPassword = _passwordHasher.Hash(command.NewPassword);
            user.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
            _outbox.Enqueue(new PasswordChanged(user.Id));
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _authSessionService.RevokeOtherUserSessionsAsync(user.Id, command.CurrentSessionId, cancellationToken);

            return Unit.Value;
        }
    }
}
