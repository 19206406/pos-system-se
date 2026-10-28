using FluentValidation;

namespace Identity.Application.Features.Auth.Queries.GetCurrentUser
{
    public sealed class GetCurrentUserValidator : AbstractValidator<GetCurrentUserQuery>
    {
        public GetCurrentUserValidator()
        {
            RuleFor(x => x.UserId).NotEmpty(); 
        }
    }
}
