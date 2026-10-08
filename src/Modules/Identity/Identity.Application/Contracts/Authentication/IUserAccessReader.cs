using Identity.Application.Dtos.Authentication;

namespace Identity.Application.Contracts.Authentication
{
    public interface IUserAccessReader
    {
        Task<UserAccessDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken); 
    }
}
