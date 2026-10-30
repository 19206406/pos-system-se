using Identity.Application.Common;
using Identity.Application.Contracts.Messaging;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Messaging.Messages;
using Shared.CQRS;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.ForgotPassword
{
    /// <summary>
    /// Always succeeds with the same response, whether or not the email exists, so it cannot be used to
    /// discover accounts. It only records the request: the token and the email are produced by the outbox
    /// processor, so the response never waits for the email. Does not revoke sessions: someone who only
    /// knows the email must not be able to log the real user out.
    /// </summary>
    public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IOutbox _outbox;
        private readonly IUnitOfWork _unitOfWork;

        public ForgotPasswordCommandHandler(IUserRepository userRepository, IOutbox outbox, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _outbox = outbox;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(EmailNormalizer.Normalize(command.Email), cancellationToken);

            // A reset is allowed even if the user has no password yet (pending invite): it replaces the invite
            if (user is null || !user.IsActive)
                return Unit.Value;

            _outbox.Enqueue(new PasswordResetRequested(user.Id));
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
