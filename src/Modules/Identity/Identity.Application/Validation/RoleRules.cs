using FluentValidation;
using Identity.Domain.Entities;

namespace Identity.Application.Validation
{
    /// <summary> Rules for the data of a role, shared by CreateRole and UpdateRole. Limits come from the entity. </summary>
    public static class RoleRules
    {
        // Far above any real catalog; only stops absurd payloads
        public const int MaxPermissionsPerRequest = 500;

        public static IRuleBuilderOptions<T, string> MustBeValidRoleName<T>(this IRuleBuilder<T, string> rule) =>
            rule.NotEmpty().WithMessage("The role name cannot be empty.")
                .MaximumLength(Role.NameMaxLength).WithMessage($"The role name cannot exceed {Role.NameMaxLength} characters.");

        public static IRuleBuilderOptions<T, string> MustBeValidRoleDescription<T>(this IRuleBuilder<T, string> rule) =>
            rule.NotEmpty().WithMessage("The role description cannot be empty.")
                .MaximumLength(Role.DescriptionMaxLength).WithMessage($"The role description cannot exceed {Role.DescriptionMaxLength} characters.");

        /// <summary> The list may be empty (a role without permissions); it may not be missing or hold blank codes. </summary>
        public static IRuleBuilderOptions<T, IReadOnlyList<string>> MustBeValidPermissionSelection<T>(this IRuleBuilder<T, IReadOnlyList<string>> rule) =>
            rule.NotNull().WithMessage("The list of permission codes is required; send an empty list for no permissions.")
                .Must(codes => codes is null || codes.Count <= MaxPermissionsPerRequest)
                .WithMessage($"A role cannot receive more than {MaxPermissionsPerRequest} permission codes.")
                .Must(codes => codes is null || codes.All(code => !string.IsNullOrWhiteSpace(code) && code.Length <= Permission.IdentifierMaxLength))
                .WithMessage($"Every permission code must have between 1 and {Permission.IdentifierMaxLength} characters.");
    }
}
