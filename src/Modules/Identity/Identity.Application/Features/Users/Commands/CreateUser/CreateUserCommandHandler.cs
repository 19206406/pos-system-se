using Identity.Application.Common;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Domain.Entities;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Users.Commands.CreateUser
{
    public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CreateUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordTokenIssuer _passwordTokenIssuer;
        private readonly TimeProvider _timeProvider;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IPasswordTokenIssuer passwordTokenIssuer,
            TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordTokenIssuer = passwordTokenIssuer;
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
                FullName = command.FullName.Trim(),
                Email = email,
                PhoneNumber = command.PhoneNumber.Trim(),
                JobTitle = command.JobTitle.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            var invite = _passwordTokenIssuer.IssueInvite(now);
            user.PasswordTokens.Add(invite.Entity);

            _userRepository.AddUser(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Pending: hand invite.PlainText to the email delivery (no delivery mechanism exists yet)
            return new CreateUserResponse(
                user.Id, user.FullName, user.JobTitle,
                user.PhoneNumber, user.Email, user.CreatedAt);
        }
    }
}
