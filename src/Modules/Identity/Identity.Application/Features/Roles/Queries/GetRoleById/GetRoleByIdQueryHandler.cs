using Identity.Application.Authorization;
using Identity.Application.Contracts.Persistence;
using Identity.Domain.Constants;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Roles.Queries.GetRoleById
{
    public sealed class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, GetRoleByIdResponse>
    {
        private readonly IRoleReadRepository _roleReadRepository;
        private readonly PermissionCatalog _permissionCatalog;

        public GetRoleByIdQueryHandler(IRoleReadRepository roleReadRepository, PermissionCatalog permissionCatalog)
        {
            _roleReadRepository = roleReadRepository;
            _permissionCatalog = permissionCatalog;
        }

        public async Task<GetRoleByIdResponse> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
        {
            var role = await _roleReadRepository.GetDetailByIdAsync(query.RoleId, cancellationToken)
                ?? throw new NotFoundException("role", query.RoleId.ToString());

            return new GetRoleByIdResponse(
                role.Id,
                role.RoleName,
                role.RoleDescription,
                role.Id == SystemRoles.AdminId,
                PermissionModuleGrouping.Group(role.Permissions, _permissionCatalog));
        }
    }
}
