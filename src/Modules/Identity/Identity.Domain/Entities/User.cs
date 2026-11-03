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

        public bool HasRole(Guid roleId) => UserRoles.Any(ur => ur.RoleId == roleId);

        /// <summary>
        /// Leaves the user with exactly <paramref name="roleIds"/>: adds the missing ones and removes the rest.
        /// Requires <see cref="UserRoles"/> to be loaded.
        /// </summary>
        public void ReplaceRoles(IReadOnlyCollection<Guid> roleIds)
        {
            var wanted = roleIds.ToHashSet();

            foreach (var current in UserRoles.Where(ur => !wanted.Contains(ur.RoleId)).ToList())
                UserRoles.Remove(current);

            var kept = UserRoles.Select(ur => ur.RoleId).ToHashSet();

            foreach (var roleId in wanted.Where(id => !kept.Contains(id)))
                UserRoles.Add(new UserRole { UserId = Id, RoleId = roleId });
        }
    }
}
