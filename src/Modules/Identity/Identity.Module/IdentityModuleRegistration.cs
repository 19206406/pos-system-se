using Identity.Application;
using Identity.Infrastructure;
using Identity.Persistence;
using Identity.Presentation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Module
{
    public static class IdentityModuleRegistration
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityApplicationServices();
            services.AddIdentityInfrastructureServices(configuration);
            services.AddIdentityPersistenceServices(configuration);
            services.AddIdentityPresentationServices();

            return services;
        }
    }
}
