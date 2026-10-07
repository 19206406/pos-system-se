using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Options
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required]
        public string Issuer { get; init; } = null!;
        [Required]
        public string Audience { get; init; } = null!;
        [Required, MinLength(32)]
        public string SigningKey { get; init; } = null!;
        [Range(1, 60)]
        public int AccessTokeLifetimeMinutes { get; init; }
        [Range(1, 90)]
        public int SessionAbsoluteLifetimeDays { get; init; }
        [Range(0, 120)] 
        public int ClockSkewSeconds { get; init; }
    }
}
