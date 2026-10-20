using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Queries.GetMySessions
{
    internal record GetMySessionsQuery(Guid Id) : IQuery<List<SessionResposeDto>>; 
}
