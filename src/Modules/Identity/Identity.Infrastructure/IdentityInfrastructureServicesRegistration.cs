using Identity.Application.Contracts.Authentication;
using Identity.Application.Contracts.Messaging;
using Identity.Application.Contracts.Security;
using Identity.Application.Options;
using Identity.Infrastructure.Email;
using Identity.Infrastructure.Maintenance;
using Identity.Infrastructure.Messaging;
using Identity.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

            AddValidatedOptions<JwtOptions>(services, configuration, JwtOptions.SectionName);
            AddValidatedOptions<PasswordTokenOptions>(services, configuration, PasswordTokenOptions.SectionName);
            AddValidatedOptions<IdentityLinksOptions>(services, configuration, IdentityLinksOptions.SectionName);
            AddValidatedOptions<SmtpOptions>(services, configuration, SmtpOptions.SectionName);
            AddValidatedOptions<OutboxOptions>(services, configuration, OutboxOptions.SectionName);
            AddValidatedOptions<RetentionOptions>(services, configuration, RetentionOptions.SectionName);
            AddValidatedOptions<BootstrapOptions>(services, configuration, BootstrapOptions.SectionName);

            // Application has no access to IOptions<T>: it receives the already validated value
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<IdentityLinksOptions>>().Value);

            services.AddSingleton<IAccessTokenGenerator, AccessTokenGenerator>();
            services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();

            services.AddSingleton<IEmailSender, SmtpEmailSender>();

            services.AddHostedService<OutboxProcessorService>();
            services.AddHostedService<RetentionPurgeService>();
            services.AddHostedService<AdminBootstrapService>();

            return services;
        }

        // Invalid configuration stops the application at startup instead of failing on first use
        private static void AddValidatedOptions<TOptions>(IServiceCollection services, IConfiguration configuration, string sectionName)
            where TOptions : class =>
            services.AddOptions<TOptions>()
                .Bind(configuration.GetSection(sectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
    }
}
