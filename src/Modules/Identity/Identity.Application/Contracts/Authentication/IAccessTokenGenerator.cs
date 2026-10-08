using Identity.Application.Dtos.Authentication;
using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Authentication
{
    public interface IAccessTokenGenerator
    {
        AccessToken Generate(User user, Guid sessionId, UserAccessDto access); 
    }
}
