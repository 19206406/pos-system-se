using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Domain.Entities;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.ResentInvite
{
    public class ResentInviteCommandHandler : ICommandHandler<ResentInviteCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly ISecureTokenGenerator _tokenGenerator;
        private readonly ITokenHasher _tokenHasher;
        private readonly IUnitOfWork _unitOfWork;

        public ResentInviteCommandHandler(
            IUserRepository userRepository, 
            IPasswordTokenRepository passwordTokenRepository, 
            ISecureTokenGenerator tokenGenerator,
            ITokenHasher tokenHasher, 
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordTokenRepository = passwordTokenRepository;
            _tokenGenerator = tokenGenerator;
            _tokenHasher = tokenHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(ResentInviteCommand command, CancellationToken cancellationToken)
        {

            var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

            if (user is null)
                throw new NotFoundException("user", command.Id.ToString());

            var now = DateTimeOffset.UtcNow;
            await _passwordTokenRepository.RevokeAllActiveByUserAsync(user.Id, now, cancellationToken); 

            var token = _tokenGenerator.GenerateToken();
            var tokenHash = _tokenHasher.Hash(token);

            var newToken = new PasswordToken
            {
                TokenHash = tokenHash,
                TokenType = "invite",
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(72),
                CreatedAt = DateTimeOffset.UtcNow
            };

            user.PasswordTokens.Add(newToken);

            await _unitOfWork.SaveChangesAsync(); 

            // send email with new Token 

            return Unit.Value; 
        }
    }
}
