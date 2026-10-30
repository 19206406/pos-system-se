using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Auth.Queries.ValidatePasswordToken
{
    public sealed class ValidatePasswordTokenQueryHandler : IQueryHandler<ValidatePasswordTokenQuery, ValidatePasswordTokenResponse>
    {
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly ITokenHasher _tokenHasher;

        public ValidatePasswordTokenQueryHandler(IPasswordTokenRepository passwordTokenRepository, ITokenHasher tokenHasher)
        {
            _passwordTokenRepository = passwordTokenRepository;
            _tokenHasher = tokenHasher;
        }

        public async Task<ValidatePasswordTokenResponse> Handle(ValidatePasswordTokenQuery query, CancellationToken cancellationToken)
        {
            var tokenHash = _tokenHasher.Hash(query.Token); 

            var passwordToken = await _passwordTokenRepository.GetByTokenAsync(tokenHash, cancellationToken);

            if (passwordToken is null)
                throw new NotFoundException("password-token", query.Token.ToString());

            if (passwordToken.UsedAt.HasValue)
                throw new ConflictException("The token is not valid to be used");

            if (passwordToken.ExpiresAt > DateTimeOffset.UtcNow)
                throw new ConflictException("The token is not valid to be used");

            return new ValidatePasswordTokenResponse(
                passwordToken.Id, passwordToken.TokenType, 
                passwordToken.CreatedAt, passwordToken.ExpiresAt); 
        }
    }
}
