using Identity.Application.Contracts.Services;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.RevokeMySession
{
    public sealed class RevokeMySessionCommandHandler : ICommandHandler<RevokeMySessionCommand>
    {
        private readonly IAuthSessionService _authSessionService;

        public RevokeMySessionCommandHandler(IAuthSessionService authSessionService)
        {
            _authSessionService = authSessionService;
        }

        public async Task<Unit> Handle(RevokeMySessionCommand command, CancellationToken cancellationToken)
        {
            // The user filter lives in the SQL update: another user's session is indistinguishable from a missing one
            var revoked = await _authSessionService
                .RevokeSessionByIdAsync(command.UserId, command.SessionId, cancellationToken);

            // Constant message (no id echoed) so foreign, missing and already revoked sessions give identical responses
            if (!revoked)
                throw new NotFoundException("The session does not exist or is no longer active.");

            return Unit.Value; 
        }
    }
}
