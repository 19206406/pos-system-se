using Identity.Application.Common;
using Identity.Application.Contracts.Messaging;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Messaging.Messages;
using Identity.Domain.Constants;
using Identity.Domain.Entities;

namespace Identity.Application.Services
{
    /// <summary>
    /// Solves the first-run deadlock: creating users needs an admin, and users are born without a password.
    /// While nobody holds the Admin role, it gives that role to the configured email (creating the user if needed)
    /// and queues an invite, so the admin sets a password through the normal set-password flow.
    /// </summary>
    public sealed class AdminBootstrapper
    {
        private readonly IUserRepository _userRepository;
        private readonly IOutbox _outbox;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public AdminBootstrapper(IUserRepository userRepository, IOutbox outbox, IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _outbox = outbox;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        /// <returns> true if it assigned the Admin role; false if an admin already existed. </returns>
        public async Task<bool> EnsureFirstAdminAsync(string email, string fullName, CancellationToken cancellationToken)
        {
            if (await _userRepository.AnyInRoleAsync(SystemRoles.AdminId, cancellationToken))
                return false;

            var normalizedEmail = EmailNormalizer.Normalize(email);
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

            if (user is null)
            {
                var now = _timeProvider.GetUtcNow().UtcDateTime;
                user = new User
                {
                    Id = Guid.CreateVersion7(),
                    FullName = fullName.Trim(),
                    Email = normalizedEmail,
                    JobTitle = "Administrador",
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _userRepository.AddUser(user);
            }

            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = SystemRoles.AdminId });

            if (user.HashPassword is null)
                _outbox.Enqueue(new UserInviteRequested(user.Id));

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
