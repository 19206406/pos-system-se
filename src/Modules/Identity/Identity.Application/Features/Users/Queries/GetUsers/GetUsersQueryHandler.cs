using Identity.Application.Contracts.Persistence;
using Shared.CQRS;

namespace Identity.Application.Features.Users.Queries.GetUsers
{
    public sealed class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, GetUsersResponse>
    {
        private readonly IUserReadRepository _userReadRepository;

        public GetUsersQueryHandler(IUserReadRepository userReadRepository)
        {
            _userReadRepository = userReadRepository;
        }

        public async Task<GetUsersResponse> Handle(GetUsersQuery query, CancellationToken cancellationToken)
        {
            var page = await _userReadRepository.GetPagedAsync(
                query.Search, query.IsActive, query.Page, query.PageSize, cancellationToken);

            var items = page.Items
                .Select(u => new GetUsersItem(u.Id, u.FullName, u.Email, u.JobTitle, u.IsActive, u.CreatedAt))
                .ToList();

            // PageSize >= 1 is guaranteed by GetUsersQueryValidator
            var totalPages = (int)Math.Ceiling(page.TotalCount / (double)query.PageSize);

            return new GetUsersResponse(items, query.Page, query.PageSize, page.TotalCount, totalPages);
        }
    }
}
