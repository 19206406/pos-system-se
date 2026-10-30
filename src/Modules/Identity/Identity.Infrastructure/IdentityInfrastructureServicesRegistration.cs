using Identity.Application.Contracts.Authentication;
using Identity.Application.Contracts.Security;
using Identity.Application.Options;
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
            services.AddSingleton<IPasswordTokenIssuer, PasswordTokenIssuer>();

            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<PasswordTokenOptions>()
                .Bind(configuration.GetSection(PasswordTokenOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IAccessTokenGenerator, AccessTokenGenerator>();
            services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>(); 

            return services; 
        } 
    }
}
