using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Options
{
    /// <summary> Lifetimes of the single-use password tokens (invite and reset). </summary>
    public sealed class PasswordTokenOptions
    {
        public const string SectionName = "PasswordTokens";

        [Range(1, 168)]
        public int InviteLifetimeHours { get; init; } = 72;

        [Range(5, 60)]
        public int ResetLifetimeMinutes { get; init; } = 30;
    }
}
