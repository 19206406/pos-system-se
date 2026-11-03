using FluentValidation;

namespace Identity.Application.Features.Roles.Queries.GetRoleById
{
    public sealed class GetRoleByIdQueryValidator : AbstractValidator<GetRoleByIdQuery>
    {
        public GetRoleByIdQueryValidator() =>
            RuleFor(x => x.RoleId).NotEmpty();
    }
}
