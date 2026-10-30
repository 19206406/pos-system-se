using FluentValidation;

namespace Identity.Application.Features.Auth.Queries.GetMySessions
{
    public sealed class GetMySessionsQueryValidator : AbstractValidator<GetMySessionsQuery>
    {
        public GetMySessionsQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.CurrentSessionId).NotEmpty();
        }
    }
}
