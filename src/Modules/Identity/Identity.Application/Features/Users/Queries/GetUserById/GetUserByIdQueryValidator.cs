using FluentValidation;

namespace Identity.Application.Features.Users.Queries.GetUserById
{
    public sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator() =>
            RuleFor(x => x.Id).NotEmpty();
    }
}
