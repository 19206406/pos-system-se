using FluentValidation;

namespace Identity.Application.Features.Users.Queries.GetUsers
{
    public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
    {
        public const int MaxPageSize = 100;
        public const int MaxSearchLength = 100;

        public GetUsersQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
            RuleFor(x => x.Search).MaximumLength(MaxSearchLength);
        }
    }
}
