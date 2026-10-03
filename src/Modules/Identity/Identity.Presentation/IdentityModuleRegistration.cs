using Identity.Application;
using Identity.Infrastructure;
using Identity.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Presentation
{
    public static class IdentityModuleRegistration
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityApplicationServices();
            services.AddIdentityInfrastructureServices(configuration); 
            services.AddIdentityPersistenceServices(configuration);

            services.AddControllers()
                .AddApplicationPart(typeof(IdentityModuleRegistration).Assembly);

            return services; 
        }
    }
}
