using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Contracts.Services;
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

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider,
            IPasswordHasher passwordHasher,
            IAuthSessionService authSessionService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _authSessionService = authSessionService;
            _passwordHasher = passwordHasher;
            _timeProvider = timeProvider;
        }

        public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
                ?? throw new NotFoundException("user", command.UserId.ToString());

            if (!user.IsActive || user.HashPassword is null)
                throw new BusinessException("No es posible cambiar la constraseña de esta cuenta");


            if (!_passwordHasher.Verify(command.CurrentPassword, user.HashPassword))
                throw new UnauthorizedException("You cannot perform the following action.");


            user.HashPassword = _passwordHasher.Hash(command.NewPassword);
            user.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _authSessionService.RevokeOtherUserSessionsAsync(user.Id, command.CurrentSessionId, cancellationToken);

            return Unit.Value;
        }
    }
}
