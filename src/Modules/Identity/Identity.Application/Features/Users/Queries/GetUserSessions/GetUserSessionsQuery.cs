using Shared.CQRS;

namespace Identity.Application.Features.Users.Queries.GetUserSessions
{
    public sealed record GetUserSessionsQuery(Guid UserId) : IQuery<GetUserSessionsResponse>;
}
