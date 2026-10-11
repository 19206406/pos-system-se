using Shared.CQRS;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.LogoutUser
{
    internal class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand>
    {
        public Task<Unit> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
