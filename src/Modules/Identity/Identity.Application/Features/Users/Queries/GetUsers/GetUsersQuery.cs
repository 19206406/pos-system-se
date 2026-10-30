using Shared.CQRS;

namespace Identity.Application.Features.Users.Queries.GetUsers
{
    public sealed record GetUsersQuery(string? Search, bool? IsActive, int Page, int PageSize)
        : IQuery<GetUsersResponse>;
}
