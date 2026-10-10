using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Domain.Entities;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Commands.ForgotPassword
{
    internal class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecureTokenGenerator _tokenGenerator;
        private readonly ITokenHasher _tokenHasher;
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository, ISecureTokenGenerator tokenGenerator, ITokenHasher tokenHasher, 
            IPasswordTokenRepository passwordTokenRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
            _tokenHasher = tokenHasher;
            _passwordTokenRepository = passwordTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

            if (user is null)
                throw new UnauthorizedException("You cannot execute this action");

            DateTimeOffset nowUtc = DateTimeOffset.UtcNow; 

            await _passwordTokenRepository.RevokeAllActiveByUserAsync(user.Id, nowUtc, cancellationToken); 

            var token = _tokenGenerator.GenerateToken();
            var tokenHash = _tokenHasher.Hash(token);

            var newToken = new PasswordToken
            {
                UserId = user.Id, 
                TokenHash = tokenHash,
                TokenType = "reset",
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(72),
                CreatedAt = DateTimeOffset.UtcNow
            };

            _passwordTokenRepository.Add(newToken); 

            await _unitOfWork.SaveChangesAsync();

            return true; 
        }
    }
}
