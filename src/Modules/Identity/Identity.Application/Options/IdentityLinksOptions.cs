using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Options
{
    /// <summary> Where the links sent by email point: the frontend page that calls set-password with the token. </summary>
    public sealed class IdentityLinksOptions
    {
        public const string SectionName = "IdentityLinks";

        [Required, Url]
        public string FrontendBaseUrl { get; init; } = null!;

        [Required]
        public string SetPasswordPath { get; init; } = "/set-password";
    }
}
