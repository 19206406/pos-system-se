using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Queries.GetCurrentUser
{
    internal record GetUserQuery(Guid Id) : IQuery<GetUserResponseDto>; 
}
