using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.RevokeMySession
{
    public sealed record RevokeMySessionCommand(Guid UserId, Guid SessionId) : ICommand; 
}
