using FluentValidation;

namespace Identity.Application.Features.Users.Commands.ResendInvite
{
    public sealed class ResendInviteCommandValidator : AbstractValidator<ResendInviteCommand>
    {
        public ResendInviteCommandValidator() =>
            RuleFor(x => x.UserId).NotEmpty();
    }
}
