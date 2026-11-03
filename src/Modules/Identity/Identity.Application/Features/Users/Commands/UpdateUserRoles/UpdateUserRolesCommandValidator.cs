using FluentValidation;

namespace Identity.Application.Features.Users.Commands.UpdateUserRoles
{
    public sealed class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
    {
        // Far above any real organization; only stops absurd payloads
        public const int MaxRolesPerRequest = 100;

        public UpdateUserRolesCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.RoleIds)
                .NotNull().WithMessage("The list of role ids is required; send an empty list to revoke every role.")
                .Must(ids => ids is null || ids.Count <= MaxRolesPerRequest)
                .WithMessage($"A user cannot receive more than {MaxRolesPerRequest} roles.");

            RuleForEach(x => x.RoleIds).NotEmpty().WithMessage("A role id cannot be empty.");
        }
    }
}
