using FluentValidation;

namespace Identity.Application.Features.Auth.Queries.GetMySessions
{
    public sealed class GetMySessionQueryValidator : AbstractValidator<GetMySessionsQuery>
    {
        public GetMySessionQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.CurrentSessionId).NotEmpty();
        }
    }
}
