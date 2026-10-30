using FluentValidation;

namespace Identity.Application.Features.Users.Queries.GetUserSessions
{
    public sealed class GetUserSessionsQueryValidator : AbstractValidator<GetUserSessionsQuery>
    {
        public GetUserSessionsQueryValidator() =>
            RuleFor(x => x.UserId).NotEmpty();
    }
}
