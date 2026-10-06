using Identity.Application.Features.Users.Commands.RegisterUser;
using Microsoft.Extensions.DependencyInjection;
using Shared.Mediator;

namespace Identity.Application
{
    public static class IdentityApplicationServicesRegistration
    {
        public static IServiceCollection AddIdentityApplicationServices(this IServiceCollection services)
        {
            // this line is only responsible for obtaining the assembly 
            // nothing else should be added for fluentvalidation validations 
            var assembly = typeof(RegisterUserCommand).Assembly;

            services.AddMediator(assembly);
            services.AddMediatorValidation(assembly); 

            return services;
        }
    }
}
