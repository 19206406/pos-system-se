using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Domain.Entities;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Commands.ForgotPassword
{
    public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecureTokenGenerator _tokenGenerator;
        private readonly ITokenHasher _tokenHasher;
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository, ISecureTokenGenerator tokenGenerator, ITokenHasher tokenHasher,
            IPasswordTokenRepository passwordTokenRepository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
            _tokenHasher = tokenHasher;
            _passwordTokenRepository = passwordTokenRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<bool> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

            if (user is null)
                throw new UnauthorizedException("You cannot execute this action");

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            await _passwordTokenRepository.RevokeAllActiveByUserAsync(user.Id, now, cancellationToken);

            var token = _tokenGenerator.GenerateToken();
            var tokenHash = _tokenHasher.Hash(token);

            var newToken = new PasswordToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                TokenType = "reset",
                ExpiresAt = now.AddHours(72),
                CreatedAt = now
            };

            _passwordTokenRepository.Add(newToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
