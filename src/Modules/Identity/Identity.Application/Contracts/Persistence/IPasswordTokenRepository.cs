using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    public interface IPasswordTokenRepository
    {
        Task<PasswordToken?> GetByTokenAsync(string tokenHash); 

        Task<List<PasswordToken>> GetAllByUserIdAsync(Guid userId); 
    }
}
