using Shared.CQRS;

namespace Identity.Application.Features.Auth.Queries.GetMySessions
{
    public sealed record GetMySessionsQuery(Guid UserId, Guid CurrentSessionId) : IQuery<GetMySessionsResponse>; 
}
