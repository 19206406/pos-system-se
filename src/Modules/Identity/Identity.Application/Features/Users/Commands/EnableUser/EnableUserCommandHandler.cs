using Identity.Application.Contracts.Persistence;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.EnableUser
{
    internal class EnableUserCommandHandler : ICommandHandler<EnableUserCommand>
    {
        private readonly IUserRepository _userRepository;

        public EnableUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(EnableUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.userId, cancellationToken);

            if (user is null)
                throw new NotFoundException("user", command.userId.ToString());

            await _userRepository.EnableAsync(user.Id, cancellationToken);

            return Unit.Value;
        }
    }
}
