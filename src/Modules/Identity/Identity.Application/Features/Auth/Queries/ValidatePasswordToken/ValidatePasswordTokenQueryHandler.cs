using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Queries.ValidatePasswordToken
{
    /// <summary>
    /// Lets the frontend check a link before showing the password form. It does not consume the token,
    /// and a missing, expired or used token give the same answer.
    /// </summary>
    public sealed class ValidatePasswordTokenQueryHandler : IQueryHandler<ValidatePasswordTokenQuery, ValidatePasswordTokenResponse>
    {
        private readonly IPasswordTokenRepository _passwordTokenRepository;
        private readonly ITokenHasher _tokenHasher;
        private readonly TimeProvider _timeProvider;

        public ValidatePasswordTokenQueryHandler(
            IPasswordTokenRepository passwordTokenRepository, ITokenHasher tokenHasher, TimeProvider timeProvider)
        {
            _passwordTokenRepository = passwordTokenRepository;
            _tokenHasher = tokenHasher;
            _timeProvider = timeProvider;
        }

        public async Task<ValidatePasswordTokenResponse> Handle(ValidatePasswordTokenQuery query, CancellationToken cancellationToken)
        {
            var passwordToken = await _passwordTokenRepository.GetByTokenHashAsync(_tokenHasher.Hash(query.Token), cancellationToken);
            var now = _timeProvider.GetUtcNow().UtcDateTime;

            if (passwordToken is null || passwordToken.UsedAt.HasValue || passwordToken.ExpiresAt <= now)
                return ValidatePasswordTokenResponse.Invalid;

            return new ValidatePasswordTokenResponse(true, passwordToken.TokenType);
        }
    }
}
