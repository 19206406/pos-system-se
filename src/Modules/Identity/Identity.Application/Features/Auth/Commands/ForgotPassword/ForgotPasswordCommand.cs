using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.ForgotPassword
{
    public sealed record ForgotPasswordCommand(string Email) : ICommand;
}
