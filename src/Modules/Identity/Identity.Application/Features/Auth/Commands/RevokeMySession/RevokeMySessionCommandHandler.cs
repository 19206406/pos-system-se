using Identity.Application.Contracts.Persistence;
using Shared.CQRS;
using Shared.Mediator;

namespace Identity.Application.Features.Auth.Commands.RevokeMySession
{
    internal class RevokeMySessionCommandHandler : ICommandHandler<RevokeMySessionCommand>
    {
        private readonly ISessionRepository _sessionRepository;

        public RevokeMySessionCommandHandler(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public Task<Unit> Handle(RevokeMySessionCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
