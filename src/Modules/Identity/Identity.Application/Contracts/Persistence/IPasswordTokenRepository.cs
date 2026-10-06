using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    public interface IPasswordTokenRepository
    {
        Task<PasswordToken?> GetPasswordToken(string tokenHash); 

        Task<List<PasswordToken>> GetPasswordTokensByUserId(Guid userId); 
    }
}
