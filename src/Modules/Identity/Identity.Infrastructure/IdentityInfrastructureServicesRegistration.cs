using Identity.Application.Contracts.Security;
using Identity.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure
{
    public static class IdentityInfrastructureServicesRegistration
    {
        public static IServiceCollection AddIdentityInfrastructureServices(IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();

            return services; 
        } 
    }
}
