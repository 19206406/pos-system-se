using Identity.Application.Contracts.Security;
using Identity.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure
{
    public static class IdentityInfrastructureServicesRegistration
    {
        public static IServiceCollection AddIdentityInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
            services.AddSingleton<ISecureTokenGenerator, RandomTokenGenerator>();
            services.AddSingleton<ITokenHasher, Sha256TokenHasher>(); 

            return services; 
        } 
    }
}
