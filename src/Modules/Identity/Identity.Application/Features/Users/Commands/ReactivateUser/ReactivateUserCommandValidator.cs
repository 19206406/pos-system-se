using FluentValidation;

namespace Identity.Application.Features.Users.Commands.ReactivateUser
{
    public sealed class ReactivateUserCommandValidator : AbstractValidator<ReactivateUserCommand>
    {
        public ReactivateUserCommandValidator() =>
            RuleFor(x => x.UserId).NotEmpty();
    }
}
