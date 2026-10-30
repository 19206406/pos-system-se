using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Services;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.DeactivateUser
{
    public sealed class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthSessionService _authSessionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public DeactivateUserCommandHandler(
            IUserRepository userRepository, 
            IAuthSessionService authSessionService, 
            IUnitOfWork unitOfWork, 
            TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _authSessionService = authSessionService;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<Unit> Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
        {
            if (command.UserId == command.RequestedByUserId)
                throw new BusinessException("You cannot deactivate your own account.");

            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
                ?? throw new NotFoundException("user", command.UserId.ToString()); 

            if (user.IsActive)
            {
                user.IsActive = false;
                user.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _unitOfWork.SaveChangesAsync(cancellationToken); 
            }

            await _authSessionService.RevokeAllUserSessionsAsync(user.Id, cancellationToken);

            return Unit.Value; 
        }
    }
}
