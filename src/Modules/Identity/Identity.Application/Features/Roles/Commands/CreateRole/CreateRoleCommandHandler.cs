using Identity.Application.Common;
using Identity.Application.Contracts.Persistence;
using Identity.Domain.Entities;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Roles.Commands.CreateRole
{
    public sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, CreateRoleResponse>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateRoleCommandHandler(IRoleRepository roleRepository, IPermissionRepository permissionRepository, IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateRoleResponse> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
        {
            var roleName = command.RoleName.Trim();

            // Checked here so the usual case is a 422; two simultaneous creations can still hit uq_roles_role_name
            if (await _roleRepository.NameExistsAsync(roleName, excludedRoleId: null, cancellationToken))
                throw new BusinessException("A role with this name already exists.");

            var permissionIds = await PermissionIdResolver.ResolveAsync(_permissionRepository, command.PermissionCodes, cancellationToken);

            var role = new Role
            {
                Id = Guid.CreateVersion7(),
                RoleName = roleName,
                RoleDescription = command.RoleDescription.Trim()
            };
            role.ReplacePermissions(permissionIds);

            _roleRepository.AddRole(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var permissionCodes = command.PermissionCodes.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
            return new CreateRoleResponse(role.Id, role.RoleName, role.RoleDescription, permissionCodes);
        }
    }
}
