using Identity.Application.Constants;
using Identity.Application.Contracts.Authentication;
using Identity.Application.Contracts.Persistence;
using Identity.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Persistence
{
    public static class IdentityPersistenceServicesRegistration
    {
        public static IServiceCollection AddIdentityPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {

            var connectionString = configuration.GetConnectionString("PosSystemDb")
                ?? throw new InvalidOperationException(
                    "The connection string 'PosSystemDb' is missing from the configuration.");

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Identity);
                    npgsql.EnableRetryOnFailure(); 
                })); 

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IdentityDbContext>());

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPasswordTokenRepository, PasswordTokenRepository>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IUserAccessReader, UserAccessReader>(); 

            return services; 
        }
    }
}
