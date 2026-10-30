using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    public interface IUserRepository 
    {
        Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Task<User?> GetWithPasswordTokensAsync(Guid userId, CancellationToken cancellationToken);
        void AddUser(User user);
    }
}
