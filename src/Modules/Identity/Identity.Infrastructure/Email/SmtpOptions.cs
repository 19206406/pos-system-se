using System.ComponentModel.DataAnnotations;

namespace Identity.Infrastructure.Email
{
    /// <summary> Development uses Mailpit (localhost:1025, no TLS, no credentials). Credentials go in secrets, never in appsettings. </summary>
    public sealed class SmtpOptions
    {
        public const string SectionName = "Smtp";

        [Required]
        public string Host { get; init; } = null!;

        [Range(1, 65535)]
        public int Port { get; init; } = 587;

        public bool EnableSsl { get; init; } = true;

        public string? UserName { get; init; }
        public string? Password { get; init; }

        [Required, EmailAddress]
        public string FromAddress { get; init; } = null!;

        [Required]
        public string FromName { get; init; } = "MiPOS";
    }
}
