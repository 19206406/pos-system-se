using FluentValidation;

namespace Identity.Application.Features.Users.Commands.RevokeUserSessions
{
    public sealed class RevokeUserSessionsCommandValidator : AbstractValidator<RevokeUserSessionsCommand>
    {
        public RevokeUserSessionsCommandValidator() =>
            RuleFor(x => x.UserId).NotEmpty();
    }
}
