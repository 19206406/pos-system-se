using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    public interface IUserRepository 
    {
        Task<User?> GetUserById(Guid id);

        Task<User?> GetUserByEmail(string email);

        Task CreateUser(User user);

        Task UpdateUser();
    }
}
