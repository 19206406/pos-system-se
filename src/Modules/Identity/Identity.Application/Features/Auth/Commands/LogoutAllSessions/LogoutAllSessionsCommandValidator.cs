using FluentValidation;

namespace Identity.Application.Features.Auth.Commands.LogoutAllSessions
{
    public sealed class LogoutAllSessionsCommandValidator : AbstractValidator<LogoutAllSessionsCommand>
    {
        public LogoutAllSessionsCommandValidator() =>
            RuleFor(x => x.UserId).NotEmpty();
    }
}
