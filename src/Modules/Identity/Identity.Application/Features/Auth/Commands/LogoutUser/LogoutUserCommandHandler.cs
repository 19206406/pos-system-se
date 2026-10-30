using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Services;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.LogoutUser
{
    public sealed class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthSessionService _sessionService;

        public LogoutUserCommandHandler(IUserRepository userRepository, IAuthSessionService sessionService)
        {
            _userRepository = userRepository;
            _sessionService = sessionService;
        }

        public async Task<Unit> Handle(LogoutUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

            if (user is null)
                throw new NotFoundException("user", command.Id.ToString());

            await _sessionService.RevokeSessionAsync(command.RefreshToken, cancellationToken);

            return Unit.Value; 
        }
    }
}
