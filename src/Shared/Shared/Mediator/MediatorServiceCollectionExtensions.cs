using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Shared.Mediator
{
    public static class MediatorServiceCollectionExtensions 
    {
        public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.TryAddScoped<ISender, Sender>(); 
        
            foreach (var assembly in assemblies.Distinct())
            {
                RegisterHandlers(services, assembly); 
            }

            return services; 
        }

        private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
        {
            var implementations = assembly.GetTypes()
                .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }); 

            foreach (var implementation in implementations)
            {
                var handlerContracts = implementation.GetInterfaces()
                    .Where(contract => contract.IsGenericType
                                       && contract.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)); 

                foreach (var contract in handlerContracts)
                {
                    var existing = services.FirstOrDefault(descriptor => descriptor.ServiceType == contract); 

                    if (existing is null)
                    {
                        services.AddScoped(contract, implementation); 
                    }
                    else if (existing.ImplementationType != implementation)
                    {
                        throw new InvalidOperationException(
                            $"'{contract.GenericTypeArguments[0].Name}' has more than one handler: " +
                            $"'{existing.ImplementationType?.FullName}' and '{implementation.FullName}"); 
                    }
                }
            }
        }
    }
}
