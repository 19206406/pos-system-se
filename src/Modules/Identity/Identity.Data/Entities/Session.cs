namespace Identity.Data.Entities
{
    public class Session
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = null!; 
        public string? DeviceInfo { get; set; } 
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public Guid? ReplacedById { get; set; }

        // navigation 
        public User User { get; set; } = null!; 
        public Session? ReplacedBy { get; set; } 
    }
}
