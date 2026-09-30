namespace Identity.Domain.Entities
{
    public class PasswordToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = null!; 
        public string TokenType { get; set; } = null!; 
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime UsedAt { get; set; }

        public User User { get; set; } = null!; 
    }
}
