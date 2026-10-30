using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Queries.ValidatePasswordToken
{
    public sealed record ValidatePasswordTokenQuery(string Token) : IQuery<GetPasswordTokenResponseDto>; 
}
