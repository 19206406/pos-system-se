using Identity.Application.Contracts.Services;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.RevokeMySession
{
    internal class RevokeMySessionCommandHandler : ICommandHandler<RevokeMySessionCommand>
    {
        private readonly IAuthSessionService _authSessionService;

        public RevokeMySessionCommandHandler(IAuthSessionService authSessionService)
        {
            _authSessionService = authSessionService;
        }

        public async Task<Unit> Handle(RevokeMySessionCommand command, CancellationToken cancellationToken)
        {
            var revoked = await _authSessionService.RevokeSessionByIdAsync(command.UserId, command.SessionId, cancellationToken)
                
            if (!revoked) 
                throw new NotFoundException("session", command.SessionId.ToString());

            return Unit.Value; 
        }
    }
}
