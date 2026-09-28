using Identity.Data.Entities;

namespace Identity.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserById(Guid id);

        Task<User?> GetUserByEmail(string email);

        Task CreateUser(User user);

        Task UpdateUser(); 
    }
}
