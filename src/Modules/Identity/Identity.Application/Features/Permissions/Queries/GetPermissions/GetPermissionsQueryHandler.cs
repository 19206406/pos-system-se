using Identity.Application.Authorization;
using Identity.Application.Contracts.Persistence;
using Shared.CQRS;

namespace Identity.Application.Features.Permissions.Queries.GetPermissions
{
    /// <summary> Every permission that can be put in a role, grouped by the module that declares it. </summary>
    public sealed class GetPermissionsQueryHandler : IQueryHandler<GetPermissionsQuery, GetPermissionsResponse>
    {
        private readonly IPermissionReadRepository _permissionReadRepository;
        private readonly PermissionCatalog _permissionCatalog;

        public GetPermissionsQueryHandler(IPermissionReadRepository permissionReadRepository, PermissionCatalog permissionCatalog)
        {
            _permissionReadRepository = permissionReadRepository;
            _permissionCatalog = permissionCatalog;
        }

        public async Task<GetPermissionsResponse> Handle(GetPermissionsQuery query, CancellationToken cancellationToken)
        {
            var permissions = await _permissionReadRepository.GetAllAsync(cancellationToken);
            return new GetPermissionsResponse(PermissionModuleGrouping.Group(permissions, _permissionCatalog));
        }
    }
}
