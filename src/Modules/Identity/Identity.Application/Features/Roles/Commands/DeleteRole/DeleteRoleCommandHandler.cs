using Identity.Application.Contracts.Persistence;
using Identity.Domain.Constants;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Roles.Commands.DeleteRole
{
    /// <summary> Deletes a role nobody holds. A role still assigned to users is a business error, never a silent revocation. </summary>
    public sealed class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand>
    {
        private const string RoleInUseMessage = "The role is assigned to one or more users; revoke it from them before deleting it.";

        private readonly IRoleRepository _roleRepository;

        public DeleteRoleCommandHandler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<Unit> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
        {
            if (!await _roleRepository.ExistsAsync(command.RoleId, cancellationToken))
                throw new NotFoundException("role", command.RoleId.ToString());

            if (command.RoleId == SystemRoles.AdminId)
                throw new BusinessException("The Admin role is managed by the system and cannot be deleted.");

            // Checked first only to give the clear message; the conditional delete below is what guarantees it
            if (await _roleRepository.HasUsersAsync(command.RoleId, cancellationToken))
                throw new BusinessException(RoleInUseMessage);

            if (!await _roleRepository.TryDeleteUnassignedAsync(command.RoleId, cancellationToken))
            {
                // Lost a race: someone assigned the role, or deleted it, between the checks and the delete
                if (!await _roleRepository.ExistsAsync(command.RoleId, cancellationToken))
                    throw new NotFoundException("role", command.RoleId.ToString());

                throw new BusinessException(RoleInUseMessage);
            }

            return Unit.Value;
        }
    }
}
