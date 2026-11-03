using Identity.Application.Contracts.Persistence;
using Identity.Domain.Constants;
using Shared.CQRS;

namespace Identity.Application.Features.Roles.Queries.GetRoles
{
    public sealed class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, GetRolesResponse>
    {
        private readonly IRoleReadRepository _roleReadRepository;

        public GetRolesQueryHandler(IRoleReadRepository roleReadRepository)
        {
            _roleReadRepository = roleReadRepository;
        }

        public async Task<GetRolesResponse> Handle(GetRolesQuery query, CancellationToken cancellationToken)
        {
            var roles = await _roleReadRepository.GetAllAsync(cancellationToken);

            var items = roles
                .Select(r => new GetRolesItem(
                    r.Id, r.RoleName, r.RoleDescription, r.Id == SystemRoles.AdminId, r.PermissionCount, r.UserCount))
                .ToList();

            return new GetRolesResponse(items);
        }
    }
}
