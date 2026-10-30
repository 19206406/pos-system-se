using FluentValidation;

namespace Identity.Application.Validation
{
    /// <summary>
    /// The single rule for a new password, shared by SetPassword and ChangePassword.
    /// The maximum keeps Argon2id from hashing huge inputs (DoS).
    /// </summary>
    public static class PasswordRules
    {
        public const int MinLength = 10;
        public const int MaxLength = 128;

        public static IRuleBuilderOptions<T, string> MustBeValidNewPassword<T>(this IRuleBuilder<T, string> rule) =>
            rule.NotEmpty().WithMessage("The password cannot be empty.")
                .MinimumLength(MinLength).WithMessage($"The password must be at least {MinLength} characters long.")
                .MaximumLength(MaxLength).WithMessage($"The password cannot exceed {MaxLength} characters.");
    }
}
