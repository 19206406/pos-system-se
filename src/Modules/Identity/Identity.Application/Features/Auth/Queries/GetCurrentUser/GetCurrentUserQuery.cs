using Shared.CQRS;

namespace Identity.Application.Features.Auth.Queries.GetCurrentUser
{
    public sealed record GetCurrentUserQuery(Guid UserId) : IQuery<GetCurrentUserResponse>; 
}
