using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Services;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.RevokeUserSessions
{
    public sealed class RevokeUserSessionsCommandHandler : ICommandHandler<RevokeUserSessionsCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthSessionService _authSessionService;

        public RevokeUserSessionsCommandHandler(IUserRepository userRepository, IAuthSessionService authSessionService)
        {
            _userRepository = userRepository;
            _authSessionService = authSessionService;
        }

        public async Task<Unit> Handle(RevokeUserSessionsCommand command, CancellationToken cancellationToken)
        {
            if (!await _userRepository.ExistAsync(command.UserId, cancellationToken))
                throw new NotFoundException("usuario", command.UserId.ToString());

            await _authSessionService.RevokeAllUserSessionAsync(command.UserId, cancellationToken);
            return Unit.Value; 
        }
    }
}
