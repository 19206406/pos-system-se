using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.ForgotPassword
{
    internal record ForgotPasswordCommand(string Email) : ICommand<bool>; 
}
