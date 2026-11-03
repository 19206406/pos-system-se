using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    public interface IRoleRepository
    {
        void AddRole(Role role); 
    }
}
