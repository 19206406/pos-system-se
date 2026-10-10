using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    public interface IPasswordTokenRepository
    {
        Task<PasswordToken?> GetByTokenAsync(string tokenHash, CancellationToken cancellationToken); 
        Task<List<PasswordToken>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        void Add(PasswordToken passwordToken);
        Task RevokeAllActiveByUserAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken); 
    }
}
