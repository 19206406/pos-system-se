using Identity.Application.DTOs.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.RefreshToken
{
    internal record class RefreshTokenCommand(Guid id, string RefreshToken) : ICommand<AuthResponseDto>;
}
