using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Services;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.LogoutAllSessions
{
    internal class LogoutAllSessionsCommandHandler : ICommandHandler<LogoutAllSessionsCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthSessionService _sessionService;

        public LogoutAllSessionsCommandHandler(IUserRepository userRepository, IAuthSessionService sessionService)
        {
            _userRepository = userRepository;
            _sessionService = sessionService;
        }

        public async Task<Unit> Handle(LogoutAllSessionsCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken)
                ?? throw new NotFoundException("user", command.Id.ToString());

            await _sessionService.RevokeAllUserSessionAsync(command.Id, cancellationToken);

            return Unit.Value; 
        }
    }
}
