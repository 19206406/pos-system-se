using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    public interface IUserRepository 
    {
        Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
        /// <summary> Expects the email already normalized with EmailNormalizer. </summary>
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
        /// <summary> Expects the email already normalized with EmailNormalizer. </summary>
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
        Task<User?> GetWithPasswordTokensAsync(Guid userId, CancellationToken cancellationToken);
        void AddUser(User user);
    }
}
