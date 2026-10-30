using Identity.Application.Contracts.Persistence;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.ReactivateUser
{
    public sealed class ReactivateUserCommandHandler : ICommandHandler<ReactivateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public ReactivateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<Unit> Handle(ReactivateUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
                       ?? throw new NotFoundException("user", command.UserId.ToString()); 

            if (!user.IsActive)
            {
                user.IsActive = true;
                user.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}
