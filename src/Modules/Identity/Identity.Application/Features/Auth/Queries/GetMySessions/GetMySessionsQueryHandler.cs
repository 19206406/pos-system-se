using Identity.Application.Contracts.Persistence;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Queries.GetMySessions
{
    public sealed class GetMySessionsQueryHandler : IQueryHandler<GetMySessionsQuery, GetMySessionsResponse>
    {
        private readonly ISessionReadRepository _sessionReadRepository;
        private readonly TimeProvider _timeProvider;

        public GetMySessionsQueryHandler(ISessionReadRepository sessionReadRepository, TimeProvider timeProvider)
        {
            _sessionReadRepository = sessionReadRepository;
            _timeProvider = timeProvider;
        }

        /// <summary>
        /// UserId and CurrentSessionId come from the "sub" and "sid" claims; CurrentSessionId
        /// is only used to flag which of the listed sessions is the one making the request.
        /// </summary>
        public async Task<GetMySessionsResponse> Handle(GetMySessionsQuery query, CancellationToken cancellationToken)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var sessions = await _sessionReadRepository.GetActiveByUserAsync(query.UserId, now, cancellationToken);

            var items = sessions
                .Select(s => new MySessionItem(
                    s.Id, s.DeviceInfo, s.IpAddress, s.CreatedAt, s.ExpiresAt,
                    IsCurrent: s.Id == query.CurrentSessionId))
                .ToList();

            return new GetMySessionsResponse(items);
        }
    }
}
