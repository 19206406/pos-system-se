using Microsoft.Extensions.DependencyInjection;

namespace Identity.Presentation
{
    public static class IdentityPresentationServicesRegistration
    {
        public static IServiceCollection AddIdentityPresentationServices(this IServiceCollection services)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(IdentityPresentationServicesRegistration).Assembly);

            return services;
        }
    }
}
