using Identity.Application.Contracts.Persistence;
using Identity.Domain.Entities;

namespace Identity.Persistence.Repositories
{
    public sealed class RoleRepository : IRoleRepository
    {
        private readonly IdentityDbContext _context;

        public RoleRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public void AddRole(Role role)
        {
            _context.Roles.Add(role); 
        }
    }
}
