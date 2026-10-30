using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Services;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.RevokeUserSessions
{
    /// <summary> Closes every session of another user without deactivating the account (lost or stolen device). </summary>
    public sealed class RevokeUserSessionsCommandHandler : ICommandHandler<RevokeUserSessionsCommand>
    {
        private readonly IUserReadRepository _userReadRepository;
        private readonly IAuthSessionService _authSessionService;

        public RevokeUserSessionsCommandHandler(IUserReadRepository userReadRepository, IAuthSessionService authSessionService)
        {
            _userReadRepository = userReadRepository;
            _authSessionService = authSessionService;
        }

        public async Task<Unit> Handle(RevokeUserSessionsCommand command, CancellationToken cancellationToken)
        {
            if (!await _userReadRepository.ExistsAsync(command.UserId, cancellationToken))
                throw new NotFoundException("user", command.UserId.ToString());

            await _authSessionService.RevokeAllUserSessionAsync(command.UserId, cancellationToken);
            return Unit.Value;
        }
    }
}
