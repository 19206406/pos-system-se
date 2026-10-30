using Identity.Application.Contracts.Persistence;
using Shared.CQRS;
using Shared.Exceptions;

namespace Identity.Application.Features.Users.Queries.GetUserSessions
{
    public sealed class GetUserSessionsQueryHandler : IQueryHandler<GetUserSessionsQuery, GetUserSessionsResponse>
    {
        private readonly IUserReadRepository _userReadRepository;
        private readonly ISessionReadRepository _sessionReadRepository;
        private readonly TimeProvider _timeProvider;

        public GetUserSessionsQueryHandler(
            IUserReadRepository userReadRepository,
            ISessionReadRepository sessionReadRepository,
            TimeProvider timeProvider)
        {
            _userReadRepository = userReadRepository;
            _sessionReadRepository = sessionReadRepository;
            _timeProvider = timeProvider;
        }

        public async Task<GetUserSessionsResponse> Handle(GetUserSessionsQuery query, CancellationToken cancellationToken)
        {
            if (!await _userReadRepository.ExistsAsync(query.UserId, cancellationToken))
                throw new NotFoundException("user", query.UserId.ToString());

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var sessions = await _sessionReadRepository.GetActiveByUserAsync(query.UserId, now, cancellationToken);

            var items = sessions
                .Select(s => new UserSessionItem(s.Id, s.DeviceInfo, s.IpAddress, s.CreatedAt, s.ExpiresAt))
                .ToList();

            return new GetUserSessionsResponse(items);
        }
    }
}
