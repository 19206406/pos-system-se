using System.ComponentModel.DataAnnotations;

namespace Identity.Infrastructure.Maintenance
{
    /// <summary> First administrator. Leave AdminEmail empty to skip the bootstrap (for example once an admin exists). </summary>
    public sealed class BootstrapOptions : IValidatableObject
    {
        public const string SectionName = "Bootstrap";

        public string? AdminEmail { get; init; }

        public string AdminFullName { get; init; } = "Administrador";

        // [EmailAddress] would reject the empty string that means "disabled", so the check is done here
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(AdminEmail) && !new EmailAddressAttribute().IsValid(AdminEmail))
                yield return new ValidationResult("Bootstrap:AdminEmail is not a valid email address.", [nameof(AdminEmail)]);
        }
    }
}
