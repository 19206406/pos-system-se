using Identity.Application.Common;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Shared.CQRS;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.ForgotPassword
{
    /// <summary>
    /// Always succeeds with the same response, whether or not the email exists, so it cannot be used to
    /// discover accounts. Does not revoke sessions: someone who only knows the email must not be able to
    /// log the real user out.
    /// </summary>
    public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly IPasswordTokenIssuer _passwordTokenIssuer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public ForgotPasswordCommandHandler(
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

        public async Task<Unit> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(EmailNormalizer.Normalize(command.Email), cancellationToken);

            // A reset is allowed even if the user has no password yet (pending invite): it replaces the invite
            if (user is null || !user.IsActive)
                return Unit.Value;

            var now = _timeProvider.GetUtcNow().UtcDateTime;

            // Only the newest link works (decision D9)
            await _passwordTokenRepository.RevokeAllActiveByUserAsync(user.Id, now, cancellationToken);

            var reset = _passwordTokenIssuer.IssueReset(now);
            reset.Entity.UserId = user.Id;
            _passwordTokenRepository.Add(reset.Entity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Pending: hand reset.PlainText to the email delivery without awaiting it (no delivery mechanism exists yet)
            return Unit.Value;
        }
    }
}
