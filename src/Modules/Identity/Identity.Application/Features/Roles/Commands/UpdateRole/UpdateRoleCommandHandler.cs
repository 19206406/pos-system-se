using Identity.Application.Common;
using Identity.Application.Contracts.Persistence;
using Identity.Domain.Constants;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Roles.Commands.UpdateRole
{
    /// <summary>
    /// Replaces the role's data and permissions in one operation. Users who hold the role get the new permissions
    /// on their next refresh (at most the access token lifetime); their sessions are not revoked.
    /// </summary>
    public sealed class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateRoleCommandHandler(IRoleRepository roleRepository, IPermissionRepository permissionRepository, IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
        {
            var role = await _roleRepository.GetByIdWithPermissionsAsync(command.RoleId, cancellationToken)
                ?? throw new NotFoundException("role", command.RoleId.ToString());

            // Admin always holds every permission (the synchronization grants new ones) and code refers to it by id
            if (role.Id == SystemRoles.AdminId)
                throw new BusinessException("The Admin role is managed by the system and cannot be modified.");

            var roleName = command.RoleName.Trim();
            if (await _roleRepository.NameExistsAsync(roleName, excludedRoleId: role.Id, cancellationToken))
                throw new BusinessException("A role with this name already exists.");

            var permissionIds = await PermissionIdResolver.ResolveAsync(_permissionRepository, command.PermissionCodes, cancellationToken);

            role.RoleName = roleName;
            role.RoleDescription = command.RoleDescription.Trim();
            role.ReplacePermissions(permissionIds);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
