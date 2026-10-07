using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Dtos.Responses;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Commands.SetUserPassword
{
    public class SetUserPasswordCommandHandler : ICommandHandler<SetUserPasswordCommand, SetUserPasswordResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenHasher _tokenHasher;

        public SetUserPasswordCommandHandler(
            IUserRepository userRepository, IPasswordTokenRepository passwordTokenRepository, IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher, ITokenHasher tokenHasher)
        {
            _userRepository = userRepository;
            _passwordTokenRepository = passwordTokenRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenHasher = tokenHasher;
        }

        public async Task<SetUserPasswordResponseDto> Handle(SetUserPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByEmail(command.Email);

            var tokenHash = _tokenHasher.Hash(command.Token);

            if (user is null || !user.IsActive)
                throw new UnauthorizedException("You cannot perform the following action.");

            var passwordToken = await _passwordTokenRepository
                .GetPasswordToken(tokenHash);

            if (passwordToken is null)
                throw new NotFoundException("password-token", command.Token.ToString());

            if (passwordToken.UsedAt.HasValue || passwordToken.ExpiresAt <= DateTimeOffset.UtcNow)
                throw new ConflictException("The token is not valid to be used");

            var password = _passwordHasher.Hash(command.Password);

            user.HashPassword = password;

            var usedAt = DateTimeOffset.UtcNow;
            passwordToken.UsedAt = usedAt;

            await _unitOfWork.SaveChangesAsync();

            return new SetUserPasswordResponseDto(user.FullName, passwordToken.TokenType, usedAt);
        }
    }
}
