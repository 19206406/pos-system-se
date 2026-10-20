using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Queries.ValidatePasswordToken
{
    internal record ValidatePasswordTokenQuery(string Token) : IQuery<GetPasswordTokenResponseDto>; 
}
