namespace Identity.Domain.Entities
{
    public class PasswordToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = null!; 
        public string TokenType { get; set; } = null!;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? UsedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public User User { get; set; } = null!; 
    }
}
