using Identity.Application.Contracts.Persistence;
using Identity.Domain.Constants;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.UpdateUserRoles
{
    /// <summary>
    /// Assigns and revokes roles in one operation. The user's permissions change on their next refresh (at most the
    /// access token lifetime); if a revocation must apply at once, revoke the user's sessions as well.
    /// </summary>
    public sealed class UpdateUserRolesCommandHandler : ICommandHandler<UpdateUserRolesCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public UpdateUserRolesCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<Unit> Handle(UpdateUserRolesCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(command.UserId, cancellationToken)
                ?? throw new NotFoundException("user", command.UserId.ToString());

            var roleIds = command.RoleIds.Distinct().ToList();

            var existingRoleIds = await _roleRepository.GetExistingIdsAsync(roleIds, cancellationToken);
            if (existingRoleIds.Count != roleIds.Count)
                throw new BusinessException("One or more of the selected roles do not exist.");

            var removesAdmin = user.HasRole(SystemRoles.AdminId) && !roleIds.Contains(SystemRoles.AdminId);

            await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
            {
                if (removesAdmin)
                {
                    // The lock makes two requests that each remove "another" admin run one after the other,
                    // so the second one sees the first change and the system never ends up without an administrator
                    await _roleRepository.LockAsync(SystemRoles.AdminId, transactionToken);

                    if (!await _userRepository.AnyOtherUserInRoleAsync(SystemRoles.AdminId, user.Id, transactionToken))
                        throw new BusinessException("The Admin role cannot be revoked from the last user who holds it.");
                }

                user.ReplaceRoles(roleIds);
                user.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _unitOfWork.SaveChangesAsync(transactionToken);
            }, cancellationToken);

            return Unit.Value;
        }
    }
}
