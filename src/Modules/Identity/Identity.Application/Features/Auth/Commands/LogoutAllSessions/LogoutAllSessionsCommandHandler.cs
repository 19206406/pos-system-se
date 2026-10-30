using Identity.Application.Contracts.Services;
using Shared.CQRS;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.LogoutAllSessions
{
    public sealed class LogoutAllSessionsCommandHandler : ICommandHandler<LogoutAllSessionsCommand>
    {
        private readonly IAuthSessionService _authSessionService;

        public LogoutAllSessionsCommandHandler(IAuthSessionService authSessionService)
        {
            _authSessionService = authSessionService;
        }

        public async Task<Unit> Handle(LogoutAllSessionsCommand command, CancellationToken cancellationToken)
        {
            await _authSessionService.RevokeAllUserSessionsAsync(command.UserId, cancellationToken); 
            return Unit.Value; 
        }
    }
}
