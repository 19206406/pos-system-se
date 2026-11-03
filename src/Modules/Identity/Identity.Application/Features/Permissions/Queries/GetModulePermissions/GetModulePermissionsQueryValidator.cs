using FluentValidation;
using Identity.Domain.Entities;

namespace Identity.Application.Features.Permissions.Queries.GetModulePermissions
{
    public sealed class GetModulePermissionsQueryValidator : AbstractValidator<GetModulePermissionsQuery>
    {
        public GetModulePermissionsQueryValidator() =>
            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("The module code cannot be empty.")
                .MaximumLength(Permission.ModuleMaxLength).WithMessage($"The module code cannot exceed {Permission.ModuleMaxLength} characters.");
    }
}
