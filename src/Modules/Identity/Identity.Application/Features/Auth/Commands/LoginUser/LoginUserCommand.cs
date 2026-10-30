using Identity.Application.Dtos.Authentication;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.LoginUser
{
    // Client (User-Agent and IP) is filled by the controller from the HTTP request
    public sealed record LoginUserCommand(string Email, string Password, ClientContext Client) : ICommand<LoginUserResponse>;
}
