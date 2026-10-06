using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Users.Querys.GetUserById
{
    public record GetUserByIdQuery(Guid Id) : IQuery<GetUserResponseDto>; 
}
