using Identity.Application.Contracts.Services;
using Identity.Application.Features.Users.Commands.CreateUser;
using Identity.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Shared.Mediator;

namespace Identity.Application
{
    public static class IdentityApplicationServicesRegistration
    {
        public static IServiceCollection AddIdentityApplicationServices(this IServiceCollection services)
        {

            /// <sumary>
            /// this line is only responsible for obtaining the assembly 
            /// nothing else should be added for fluentvalidation validations 
            /// </sumary>
            var assembly = typeof(CreateUserCommand).Assembly;

            services.AddMediator(assembly);
            services.AddMediatorValidation(assembly);

            services.AddScoped<IAuthSessionService, AuthSessionService>(); 

            return services;
        }
    }
}
