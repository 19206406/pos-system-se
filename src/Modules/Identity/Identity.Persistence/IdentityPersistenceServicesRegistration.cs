using Identity.Application.Contracts.Persistence;
using Identity.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Persistence
{
    public static class IdentityPersistenceServicesRegistration
    {
        public static IServiceCollection AddIdentityPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("PosSystemConnectionString"));
            });

            // the implementation is missing, it is better to separate it from the Context although I am not sure
            services.AddScoped<IUnitOfWork>();

            services.AddScoped<IUserRepository, UserRepository>();

            return services; 
        }
    }
}
