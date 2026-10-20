using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Services;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.DisableUser
{
    internal class DisableUserCommandHandler : ICommandHandler<DisableUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthSessionService _sessionService;

        public DisableUserCommandHandler(IUserRepository userRepository, IAuthSessionService sessionService)
        {
            _userRepository = userRepository;
            _sessionService = sessionService;
        }

        public async Task<Unit> Handle(DisableUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.userId, cancellationToken);

            if (user is null)
                throw new NotFoundException("user", command.userId.ToString());

            await _userRepository.DisableAsync(user.Id, cancellationToken);
            await _sessionService.RevokeAllUserSessionAsync(user.Id, cancellationToken);

            return Unit.Value; 
        }
    }
}
