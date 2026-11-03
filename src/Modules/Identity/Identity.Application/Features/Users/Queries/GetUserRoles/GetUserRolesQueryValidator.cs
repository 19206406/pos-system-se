using FluentValidation;

namespace Identity.Application.Features.Users.Queries.GetUserRoles
{
    public sealed class GetUserRolesQueryValidator : AbstractValidator<GetUserRolesQuery>
    {
        public GetUserRolesQueryValidator() =>
            RuleFor(x => x.UserId).NotEmpty();
    }
}
