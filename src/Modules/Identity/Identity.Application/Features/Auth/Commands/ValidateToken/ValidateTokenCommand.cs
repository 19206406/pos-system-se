using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.ValidateToken
{
    internal record ValidateTokenCommand(string Token) : ICommand<GetPasswordTokenResponseDto>; 
}
