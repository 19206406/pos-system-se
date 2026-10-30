using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.ResendInvite
{
    public sealed class ResendInviteCommandHandler : ICommandHandler<ResendInviteCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly IPasswordTokenIssuer _passwordTokenIssuer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public ResendInviteCommandHandler(
            IUserRepository userRepository,
            IPasswordTokenRepository passwordTokenRepository,
            IPasswordTokenIssuer passwordTokenIssuer,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _passwordTokenRepository = passwordTokenRepository;
            _passwordTokenIssuer = passwordTokenIssuer;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<Unit> Handle(ResendInviteCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
                ?? throw new NotFoundException("user", command.UserId.ToString());

            // A user who already has a password recovers access through forgot-password, not through an invite
            if (user.HashPassword is not null)
                throw new BusinessException("The user already has a password; an invite cannot be resent.");

            var now = _timeProvider.GetUtcNow().UtcDateTime;

            // Only the newest link works (decision D9)
            await _passwordTokenRepository.RevokeAllActiveByUserAsync(user.Id, now, cancellationToken);

            var invite = _passwordTokenIssuer.IssueInvite(now);
            invite.Entity.UserId = user.Id;
            _passwordTokenRepository.Add(invite.Entity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Pending: hand invite.PlainText to the email delivery (no delivery mechanism exists yet)
            return Unit.Value;
        }
    }
}
