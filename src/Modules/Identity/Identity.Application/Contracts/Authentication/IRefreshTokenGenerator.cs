using Identity.Application.Dtos.Authentication;

namespace Identity.Application.Contracts.Authentication
{
    public interface IRefreshTokenGenerator
    {
        GeneratedRefreshToken Generate();
    }
}
