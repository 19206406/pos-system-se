using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Dtos.Responses;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Commands.ValidateToken
{
    internal class ValidateTokenCommandHandler : ICommandHandler<ValidateTokenCommand, GetPasswordTokenResponseDto>
    {
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly ITokenHasher _tokenHasher;

        public ValidateTokenCommandHandler(IPasswordTokenRepository passwordTokenRepository, ITokenHasher tokenHasher)
        {
            _passwordTokenRepository = passwordTokenRepository;
            _tokenHasher = tokenHasher;
        }

        public async Task<GetPasswordTokenResponseDto> Handle(ValidateTokenCommand command, CancellationToken cancellationToken)
        {
            var tokenHash = _tokenHasher.Hash(command.Token); 

            var passwordToken = await _passwordTokenRepository.GetPasswordToken(tokenHash);

            if (passwordToken is null)
                throw new NotFoundException("password-token", command.Token.ToString());

            if (passwordToken.UsedAt.HasValue)
                throw new ConflictException("The token is not valid to be used");

            if (passwordToken.ExpiresAt > DateTimeOffset.UtcNow)
                throw new ConflictException("The token is not valid to be used");

            return new GetPasswordTokenResponseDto(
                passwordToken.Id, passwordToken.TokenType, 
                passwordToken.CreatedAt, passwordToken.ExpiresAt); 
        }
    }
}
