using Identity.Application.Contracts.Persistence;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.Enable
{
    internal class EnableCommandHandler : ICommandHandler<EnableCommand>
    {
        private readonly IUserRepository _userRepository;

        public EnableCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(EnableCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.userId, cancellationToken);

            if (user is null)
                throw new NotFoundException("user", command.userId.ToString());

            await _userRepository.EnableAsync(user.Id, cancellationToken);

            return Unit.Value;
        }
    }
}
