using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Queries.ValidateToken
{
    internal record ValidateTokenCommand(string Token) : IQuery<GetPasswordTokenResponseDto>; 
}
