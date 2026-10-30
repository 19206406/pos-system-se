using FluentValidation;

namespace Identity.Application.Features.Users.Commands.CreateUser
{
    public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("The new user's name cannot be empty.")
                .MaximumLength(100).WithMessage("The new user's name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("The new user's email address cannot be empty.")
                .EmailAddress().WithMessage("The value you entered is not a valid email address.")
                .MaximumLength(150).WithMessage("The email cannot exceed 150 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("The user's phone number must contain a value.")
                .MaximumLength(20).WithMessage("The user's phone number cannot exceed 20 characters.");

            RuleFor(x => x.JobTitle)
                .NotEmpty().WithMessage("The user must hold a position within the organization.")
                .MaximumLength(100).WithMessage("The job title cannot exceed 100 characters.");
        }
    }
}
