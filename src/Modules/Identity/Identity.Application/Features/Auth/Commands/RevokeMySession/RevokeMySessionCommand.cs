using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.RevokeMySession
{
    internal record RevokeMySessionCommand(Guid Id) : ICommand; 
}
