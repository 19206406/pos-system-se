using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    public interface IUserRepository 
    {
        Task<User?> GetByIdAsync(Guid id);

        Task<User?> GetByEmailAsync(string email);

        void AddUser(User user);

        Task<User?> GetWithPasswordTokensAsync(Guid id); 
    }
}
