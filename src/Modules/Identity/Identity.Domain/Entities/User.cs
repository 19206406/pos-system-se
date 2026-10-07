namespace Identity.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!; 
        public string? PhoneNumber { get; set; }
        public string JobTitle { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsActive { get; set; } = true; 
        public string? HashPassword { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // navegation 
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<PasswordToken> PasswordTokens { get; set; } = new List<PasswordToken>(); 
    }
}
