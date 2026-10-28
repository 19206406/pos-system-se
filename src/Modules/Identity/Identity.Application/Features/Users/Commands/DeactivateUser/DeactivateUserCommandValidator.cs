using FluentValidation;

namespace Identity.Application.Features.Users.Commands.DeactivateUser
{
    public sealed class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
    {
        public DeactivateUserCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.RequestedByUserId).NotEmpty();
        }
    }
}
