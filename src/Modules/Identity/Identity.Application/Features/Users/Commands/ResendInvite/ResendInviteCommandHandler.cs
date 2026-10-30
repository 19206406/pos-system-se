using Identity.Application.Contracts.Messaging;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Messaging.Messages;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.ResendInvite
{
    public sealed class ResendInviteCommandHandler : ICommandHandler<ResendInviteCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IOutbox _outbox;
        private readonly IUnitOfWork _unitOfWork;

        public ResendInviteCommandHandler(IUserRepository userRepository, IOutbox outbox, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _outbox = outbox;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(ResendInviteCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
                ?? throw new NotFoundException("user", command.UserId.ToString());

            // A user who already has a password recovers access through forgot-password, not through an invite
            if (user.HashPassword is not null)
                throw new BusinessException("The user already has a password; an invite cannot be resent.");

            // Delivery invalidates the earlier invites and issues the new token (decision D9)
            _outbox.Enqueue(new UserInviteRequested(user.Id));
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
