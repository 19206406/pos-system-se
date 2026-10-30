using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Domain.Entities;
using Shared.CQRS;
using Shared.Exceptions;
using Shared.Mediator;

namespace Identity.Application.Features.Users.Commands.ResendInvite
{
    public sealed class ResendInviteCommandHandler : ICommandHandler<ResendInviteCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly ISecureTokenGenerator _tokenGenerator;
        private readonly ITokenHasher _tokenHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public ResendInviteCommandHandler(
            IUserRepository userRepository, 
            IPasswordTokenRepository passwordTokenRepository, 
            ISecureTokenGenerator tokenGenerator,
            ITokenHasher tokenHasher, 
            IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _passwordTokenRepository = passwordTokenRepository;
            _tokenGenerator = tokenGenerator;
            _tokenHasher = tokenHasher;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<Unit> Handle(ResendInviteCommand command, CancellationToken cancellationToken)
        {

            var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

            if (user is null)
                throw new NotFoundException("user", command.Id.ToString());

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            await _passwordTokenRepository.RevokeAllActiveByUserAsync(user.Id, now, cancellationToken); 

            var token = _tokenGenerator.GenerateToken();
            var tokenHash = _tokenHasher.Hash(token);

            var newToken = new PasswordToken
            {
                TokenHash = tokenHash,
                TokenType = "invite",
                ExpiresAt = now.AddHours(72),
                CreatedAt = now
            };

            user.PasswordTokens.Add(newToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken); 

            // send email with new Token 

            return Unit.Value; 
        }
    }
}
