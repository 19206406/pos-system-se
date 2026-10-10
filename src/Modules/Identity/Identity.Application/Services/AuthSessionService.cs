using Identity.Application.Contracts.Authentication;
using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Security;
using Identity.Application.Contracts.Services;
using Identity.Application.Dtos.Authentication;
using Identity.Domain.Entities;

namespace Identity.Application.Services
{
    internal sealed class AuthSessionService : IAuthSessionService
    {
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
                Console.WriteLine("I need a new Exception to token");

            var now = Now();
            var current = await _sessionRepository.GetByTokenHashAsync(_tokenHasher.Hash(refreshToken), cancellationToken)
                ?? throw new Exception(); 

            if (current.RevokedAt.HasValue)
            {
                if (current.ReplacedById.HasValue)
                    await _sessionRepository.RevokeAllActiveByUserAsync(current.UserId, now, cancellationToken);

                throw new Exception(); 
            }

            if (now >= current.ExpiresAt)
                throw new Exception(); 

            if (!current.User.IsActive)
            {
                current.RevokedAt = now;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new Exception(); 
            }

            var access = await _userAccessReader.GetByUserIdAsync(current.UserId, cancellationToken);
            var nextRefreshToken = _refreshTokenGenerator.Generate();

            var session = new Session
            {
                UserId = current.UserId,
                TokenHash = nextRefreshToken.Hash,
                DeviceInfo = client.DeviceInfo,
                IpAddress = client.IpAddress,
                CreatedAt = now,
                ExpiresAt = current.ExpiresAt
            };

            await _sessionRepository.AddAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync();

            var rotated = await _sessionRepository.TryMarkReplacedAsync(current.Id, session.Id, now, cancellationToken); 
            if (!rotated)
            {
                session.RevokedAt = now;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new Exception(); 
            }

            var accessToken = _accessTokenGenerator.Generate(current.User, session.Id, access);

            return new AuthTokensDto(
                accessToken.Value, accessToken.ExpiresAt, 
                nextRefreshToken.PlainText, session.ExpiresAt); 
        }

        public async Task RevokeAllUserSessionAsync(Guid userId, CancellationToken cancellationToken)
        {
            await _sessionRepository.RevokeAllActiveByUserAsync(userId, Now(), cancellationToken); 
        }

        public async Task RevokeSessionAsync(string refreshToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return;

            var session = await _sessionRepository.GetByTokenHashAsync(_tokenHasher.Hash(refreshToken), cancellationToken);
            if (session is null || Now() >= session.RevokedAt) return;

            session.RevokedAt = Now();
            await _unitOfWork.SaveChangesAsync(); 
        }

        private DateTime Now() => _timeProvider.GetUtcNow().UtcDateTime; 
    }
}
