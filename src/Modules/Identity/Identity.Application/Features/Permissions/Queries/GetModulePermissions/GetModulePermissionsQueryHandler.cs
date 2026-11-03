using Identity.Application.Authorization;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Dtos.Permissions;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Permissions.Queries.GetModulePermissions
{
    /// <summary>
    /// The permissions of one module. A module exists if a catalog declares it (even before the synchronization has
    /// written its rows, when the list is empty) or if the table still holds permissions for it; otherwise 404.
    /// </summary>
    public sealed class GetModulePermissionsQueryHandler : IQueryHandler<GetModulePermissionsQuery, GetModulePermissionsResponse>
    {
        private readonly IPermissionReadRepository _permissionReadRepository;
        private readonly PermissionCatalog _permissionCatalog;

        public GetModulePermissionsQueryHandler(IPermissionReadRepository permissionReadRepository, PermissionCatalog permissionCatalog)
        {
            _permissionReadRepository = permissionReadRepository;
            _permissionCatalog = permissionCatalog;
        }

        public async Task<GetModulePermissionsResponse> Handle(GetModulePermissionsQuery query, CancellationToken cancellationToken)
        {
            // Module codes are lowercase by catalog validation, so "Identity" finds "identity"
            var moduleCode = query.ModuleCode.Trim().ToLowerInvariant();

            var permissions = await _permissionReadRepository.GetByModuleAsync(moduleCode, cancellationToken);

            if (permissions.Count == 0 && !_permissionCatalog.IsDeclaredModule(moduleCode))
                throw new NotFoundException("module", moduleCode);

            return new GetModulePermissionsResponse(
                moduleCode,
                _permissionCatalog.GetModuleName(moduleCode),
                permissions.Select(p => new PermissionItemDto(p.Code, p.Name, p.Description)).ToList());
        }
    }
}
