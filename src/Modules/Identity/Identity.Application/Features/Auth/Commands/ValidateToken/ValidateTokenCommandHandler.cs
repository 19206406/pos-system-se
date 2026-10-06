using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.ValidateToken
{
    internal class ValidateTokenCommandHandler : ICommandHandler<ValidateTokenCommand, bool>
    {
        public Task<bool> Handle(ValidateTokenCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
