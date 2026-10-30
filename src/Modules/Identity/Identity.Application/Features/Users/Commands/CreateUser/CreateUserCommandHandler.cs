using Identity.Application.Common;
using Identity.Application.Contracts.Messaging;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Messaging.Messages;
using Identity.Domain.Entities;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Users.Commands.CreateUser
{
    public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CreateUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOutbox _outbox;
        private readonly TimeProvider _timeProvider;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IOutbox outbox,
            TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _outbox = outbox;
            _timeProvider = timeProvider;
        }

        public async Task<CreateUserResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
            var email = EmailNormalizer.Normalize(command.Email);

            // Checked here so the usual case is a 422; two simultaneous creations can still hit uq_users_email
            if (await _userRepository.EmailExistsAsync(email, cancellationToken))
                throw new BusinessException("A user with this email address already exists.");

            var now = _timeProvider.GetUtcNow().UtcDateTime;

            var user = new User
            {
                // Assigned here, not by the database, because the invite message needs it before saving
                Id = Guid.CreateVersion7(),
                FullName = command.FullName.Trim(),
                Email = email,
                PhoneNumber = command.PhoneNumber.Trim(),
                JobTitle = command.JobTitle.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            _userRepository.AddUser(user);

            // Same transaction as the user: the invite is sent if and only if the user was created
            _outbox.Enqueue(new UserInviteRequested(user.Id));
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateUserResponse(
                user.Id, user.FullName, user.JobTitle,
                user.PhoneNumber, user.Email, user.CreatedAt);
        }
    }
}
