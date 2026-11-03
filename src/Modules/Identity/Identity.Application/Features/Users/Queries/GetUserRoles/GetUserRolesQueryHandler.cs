using Identity.Application.Contracts.Persistence;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Users.Queries.GetUserRoles
{
    public sealed class GetUserRolesQueryHandler : IQueryHandler<GetUserRolesQuery, GetUserRolesResponse>
    {
        private readonly IUserReadRepository _userReadRepository;
        private readonly IRoleReadRepository _roleReadRepository;

        public GetUserRolesQueryHandler(IUserReadRepository userReadRepository, IRoleReadRepository roleReadRepository)
        {
            _userReadRepository = userReadRepository;
            _roleReadRepository = roleReadRepository;
        }

        public async Task<GetUserRolesResponse> Handle(GetUserRolesQuery query, CancellationToken cancellationToken)
        {
            // A user without roles and a user that does not exist must not look the same
            if (!await _userReadRepository.ExistsAsync(query.UserId, cancellationToken))
                throw new NotFoundException("user", query.UserId.ToString());

            var roles = await _roleReadRepository.GetByUserIdAsync(query.UserId, cancellationToken);

            return new GetUserRolesResponse(
                query.UserId,
                roles.Select(r => new GetUserRolesItem(r.Id, r.RoleName)).ToList());
        }
    }
}
