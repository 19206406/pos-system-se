using Identity.Data.Entities;

namespace Identity.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserById(Guid id);

        Task<User?> GetUserByEmail(string email);

        Task CreateUser(User user);

        Task UpdateUser(); 
    }
}
