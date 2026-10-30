using FluentValidation;

namespace Identity.Application.Features.Auth.Queries.GetCurrentUser
{
    public sealed class GetCurrentUserQueryValidator : AbstractValidator<GetCurrentUserQuery>
    {
        public GetCurrentUserQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty(); 
        }
    }
}
