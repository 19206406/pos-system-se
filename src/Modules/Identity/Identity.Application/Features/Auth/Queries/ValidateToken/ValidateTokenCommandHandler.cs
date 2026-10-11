using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Dtos.Responses;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Queries.ValidateToken
{
    internal class ValidateTokenCommandHandler : IQueryHandler<ValidateTokenCommand, GetPasswordTokenResponseDto>
    {
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly ITokenHasher _tokenHasher;

        public ValidateTokenCommandHandler(IPasswordTokenRepository passwordTokenRepository, ITokenHasher tokenHasher)
        {
            _passwordTokenRepository = passwordTokenRepository;
            _tokenHasher = tokenHasher;
        }

        public async Task<GetPasswordTokenResponseDto> Handle(ValidateTokenCommand query, CancellationToken cancellationToken)
        {
            var tokenHash = _tokenHasher.Hash(query.Token); 

            var passwordToken = await _passwordTokenRepository.GetByTokenAsync(tokenHash, cancellationToken);

            if (passwordToken is null)
                throw new NotFoundException("password-token", query.Token.ToString());

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
