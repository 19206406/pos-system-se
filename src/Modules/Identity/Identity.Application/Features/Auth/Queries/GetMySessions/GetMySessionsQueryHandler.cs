using Identity.Application.Contracts.Persistence;
using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Queries.GetMySessions
{
    internal class GetMySessionsQueryHandler : IQueryHandler<GetMySessionsQuery, GetMySessionsResponse>
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly TimeProvider _timeProvider;

        public GetMySessionsQueryHandler(ISessionRepository sessionRepository, TimeProvider timeProvider)
        {
            _sessionRepository = sessionRepository;
            _timeProvider = timeProvider;
        }

        /// <summary>
        /// This action depends directly on the information contained in the JWT; 
        /// without it, the action cannot be executed because the endpoint does not receive any parameters, 
        /// either in the body or in the header. 
        /// </summary>
        public async Task<GetMySessionsResponse> Handle(GetMySessionsQuery query, CancellationToken cancellationToken)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var sessions = await _sessionRepository.GetActiveByUserAsync(query.UserId, now, cancellationToken);

            var items = sessions
                .Select(s => new MySessionItem(
                    s.Id, s.DeviceInfo, s.IpAddress, s.CreatedAt, s.ExpiresAt,
                    IsCurrent: s.Id == query.CurrentSessionId))
                .ToList();

            return new GetMySessionsResponse(items);
        }
    }
}
