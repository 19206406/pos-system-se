using FluentValidation;

namespace Identity.Application.Features.Auth.Commands.RevokeMySession
{
    public sealed class RevokeMySessionCommandValidator : AbstractValidator<RevokeMySessionCommand>
    {
        public RevokeMySessionCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty(); 
            RuleFor(x => x.SessionId).NotEmpty();
        }
    }
}
