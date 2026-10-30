namespace Identity.Domain.Entities
{
    public class Session
    {
        public const int DeviceInfoMaxLength = 100;
        public const int IpAddressMaxLength = 50;

        private string? _deviceInfo;
        private string? _ipAddress;

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = null!;

        /// <summary> Client User-Agent, truncated: real ones often exceed the column length. </summary>
        public string? DeviceInfo
        {
            get => _deviceInfo;
            set => _deviceInfo = Truncate(value, DeviceInfoMaxLength);
        }

        public string? IpAddress
        {
            get => _ipAddress;
            set => _ipAddress = Truncate(value, IpAddressMaxLength);
        }

        public DateTime CreatedAt { get; set; } 
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public Guid? ReplacedById { get; set; }

        // navigation 
        public User User { get; set; } = null!; 
        public Session? ReplacedBy { get; set; }

        private static string? Truncate(string? value, int maxLength) =>
            value is not null && value.Length > maxLength ? value[..maxLength] : value;
    }
}
