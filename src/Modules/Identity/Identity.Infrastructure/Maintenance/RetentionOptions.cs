using System.ComponentModel.DataAnnotations;

namespace Identity.Infrastructure.Maintenance
{
    public sealed class RetentionOptions
    {
        public const string SectionName = "Retention";

        /// <summary> Expired, revoked or used rows are kept this long (for auditing and reuse detection) before deletion. </summary>
        [Range(1, 365)]
        public int RetentionDays { get; init; } = 30;

        [Range(5, 1440)]
        public int IntervalMinutes { get; init; } = 60;
    }
}
