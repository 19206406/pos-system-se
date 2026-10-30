using Identity.Application.Contracts.Authentication;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Contracts.Services;
using Identity.Application.Dtos.Authentication;
using Identity.Domain.Entities;
using Shared.Exceptions;

namespace Identity.Application.Services
{
    internal sealed class AuthSessionService : IAuthSessionService
    {
        // One message for every refresh failure so the response never reveals the cause
        private const string InvalidRefreshTokenMessage = "The refresh token is invalid or has expired.";

        private readonly ISessionRepository _sessionRepository;
        private readonly IUserAccessReader _userAccessReader;
        private readonly IAccessTokenGenerator _accessTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly ITokenHasher _tokenHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public AuthSessionService(
            ISessionRepository sessionRepository, IUserAccessReader userAccessReader, IAccessTokenGenerator accessTokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator, ITokenHasher tokenHasher, IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _sessionRepository = sessionRepository;
            _userAccessReader = userAccessReader;
            _accessTokenGenerator = accessTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _tokenHasher = tokenHasher;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<AuthTokensDto> CreateSessionAsync(User user, ClientContext client, CancellationToken cancellationToken)
        {
            var now = Now();
            var access = await _userAccessReader.GetByUserIdAsync(user.Id, cancellationToken);
            var refreshToken = _refreshTokenGenerator.Generate();

            var session = new Session
            {
                UserId = user.Id,
                TokenHash = refreshToken.Hash,
                DeviceInfo = client.DeviceInfo,
                IpAddress = client.IpAddress,
                CreatedAt = now,
                ExpiresAt = refreshToken.ExpiresAt
            };

            await _sessionRepository.AddAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var accessToken = _accessTokenGenerator.Generate(user, session.Id, access);

            return new AuthTokensDto(
                accessToken.Value, accessToken.ExpiresAt,
                refreshToken.PlainText, session.ExpiresAt);
        }

        public async Task<AuthTokensDto> RefreshSessionAsync(string refreshToken, ClientContext client, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new UnauthorizedException(InvalidRefreshTokenMessage);

            var now = Now();
            var current = await _sessionRepository.GetByTokenHashAsync(_tokenHasher.Hash(refreshToken), cancellationToken)
                ?? throw new UnauthorizedException(InvalidRefreshTokenMessage);

            if (current.RevokedAt.HasValue)
            {
                // A token that was already rotated showing up again means it was probably stolen.
                // A token revoked by logout (no ReplacedById) is simply rejected.
                if (current.ReplacedById.HasValue)
                    await _sessionRepository.RevokeAllActiveByUserAsync(current.UserId, now, cancellationToken);

                throw new UnauthorizedException(InvalidRefreshTokenMessage);
            }

            if (now >= current.ExpiresAt)
                throw new UnauthorizedException(InvalidRefreshTokenMessage);

            if (!current.User.IsActive)
            {
                current.RevokedAt = now;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedException(InvalidRefreshTokenMessage);
            }

            // Roles and permissions are read again so role changes reach the token on every refresh
            var access = await _userAccessReader.GetByUserIdAsync(current.UserId, cancellationToken);
            var nextRefreshToken = _refreshTokenGenerator.Generate();

            // Absolute expiration: the new session inherits ExpiresAt from the one it replaces
            var next = new Session
            {
                UserId = current.UserId,
                TokenHash = nextRefreshToken.Hash,
                DeviceInfo = client.DeviceInfo ?? current.DeviceInfo,
                IpAddress = client.IpAddress,
                CreatedAt = now,
                ExpiresAt = current.ExpiresAt
            };

            await _sessionRepository.AddAsync(next, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var rotated = await _sessionRepository.TryMarkAsReplacedAsync(current.Id, next.Id, now, cancellationToken);
            if (!rotated)
            {
                // Another request used the same refresh token at the same time and won the race
                next.RevokedAt = now;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedException(InvalidRefreshTokenMessage);
            }

            var accessToken = _accessTokenGenerator.Generate(current.User, next.Id, access);

            return new AuthTokensDto(
                accessToken.Value, accessToken.ExpiresAt,
                nextRefreshToken.PlainText, next.ExpiresAt);
        }

        public async Task RevokeSessionAsync(string refreshToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return;

            var session = await _sessionRepository.GetByTokenHashAsync(_tokenHasher.Hash(refreshToken), cancellationToken);
            if (session is null || session.RevokedAt.HasValue) return;

            session.RevokedAt = Now();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> RevokeSessionByIdAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken) =>
            await _sessionRepository.RevokeActiveByIdAsync(userId, sessionId, Now(), cancellationToken);

        public async Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken) =>
            await _sessionRepository.RevokeAllActiveByUserAsync(userId, Now(), cancellationToken);

        public async Task RevokeOtherUserSessionsAsync(Guid userId, Guid currentSessionId, CancellationToken cancellationToken) =>
            await _sessionRepository.RevokeAllActiveByUserExceptAsync(userId, currentSessionId, Now(), cancellationToken);

        private DateTime Now() => _timeProvider.GetUtcNow().UtcDateTime;
    }
}
