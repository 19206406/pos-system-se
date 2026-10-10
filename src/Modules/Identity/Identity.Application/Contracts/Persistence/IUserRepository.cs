using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    public interface IUserRepository 
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

        void AddUser(User user);

        Task<User?> GetWithPasswordTokensAsync(Guid id, CancellationToken cancellationToken); 
    }
}
