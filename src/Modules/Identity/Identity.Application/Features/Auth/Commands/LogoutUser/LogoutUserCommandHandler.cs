using Identity.Application.Contracts.Services;
using Shared.CQRS;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.LogoutUser
{
    /// <summary>
    /// Idempotent and silent: unknown, empty or already revoked tokens are ignored and the endpoint always
    /// answers 204, so it reveals nothing about the token. Deliberately has no validator for the same reason.
    /// </summary>
    public sealed class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand>
    {
        private readonly IAuthSessionService _sessionService;

        public LogoutUserCommandHandler(IAuthSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public async Task<Unit> Handle(LogoutUserCommand command, CancellationToken cancellationToken)
        {
            await _sessionService.RevokeSessionAsync(command.RefreshToken, cancellationToken);
            return Unit.Value;
        }
    }
}
