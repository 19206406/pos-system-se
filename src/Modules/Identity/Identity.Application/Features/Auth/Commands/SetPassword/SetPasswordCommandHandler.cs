using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Commands.SetPassword
{
    public sealed class SetPasswordCommandHandler : ICommandHandler<SetPasswordCommand, SetPasswordResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenHasher _tokenHasher;
        private readonly TimeProvider _timeProvider;

        public SetPasswordCommandHandler(
            IUserRepository userRepository, IPasswordTokenRepository passwordTokenRepository, IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher, ITokenHasher tokenHasher, TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _passwordTokenRepository = passwordTokenRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenHasher = tokenHasher;
            _timeProvider = timeProvider;
        }

        public async Task<SetPasswordResponse> Handle(SetPasswordCommand command, CancellationToken cancellationToken)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

            var tokenHash = _tokenHasher.Hash(command.Token);

            if (user is null || !user.IsActive)
                throw new UnauthorizedException("You cannot perform the following action");

            var passwordToken = await _passwordTokenRepository.GetByTokenAsync(tokenHash, cancellationToken);

            if (passwordToken is null)
                throw new UnauthorizedException("You cannot perform the following action");

            if (passwordToken.UsedAt.HasValue || passwordToken.ExpiresAt <= now || passwordToken.TokenType != "invite")
                throw new InvalidTokenException("The token is not valid to be used");

            var password = _passwordHasher.Hash(command.Password);

            user.HashPassword = password;

            var usedAt = now;
            passwordToken.UsedAt = usedAt;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SetPasswordResponse(user.FullName, command.Token, passwordToken.TokenType, usedAt);
        }
    }
}
