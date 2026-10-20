using Identity.Application.Contracts.Persistence;
using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Queries.GetMySessions
{
    internal class GetMySessionsQueryHandler : IQueryHandler<GetMySessionsQuery, List<SessionResposeDto>>
    {
        private readonly ISessionRepository _sessionRepository;

        public GetMySessionsQueryHandler(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        /// <summary>
        /// This action depends directly on the information contained in the JWT; 
        /// without it, the action cannot be executed because the endpoint does not receive any parameters, 
        /// either in the body or in the header. 
        /// </summary>
        public async Task<List<SessionResposeDto>> Handle(GetMySessionsQuery query, CancellationToken cancellationToken)
        {
            var sessions = await _sessionRepository.GetAllByUserAsync(query.Id, cancellationToken);

            List<SessionResposeDto> response = new List<SessionResposeDto>(); 

            foreach(var s in sessions)
                response.Add(new SessionResposeDto(
                    s.Id, 
                    s.UserId, 
                    s.DeviceInfo, 
                    s.IpAddress, 
                    s.CreatedAt, 
                    s.ExpiresAt));

            return response; 
        }
    }
}
