using Shared.CQRS;

namespace Identity.Application.Features.Users.Queries.GetUserRoles
{
    public sealed record GetUserRolesQuery(Guid UserId) : IQuery<GetUserRolesResponse>;
}
