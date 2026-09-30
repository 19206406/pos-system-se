using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Shared.Mediator
{
    public static class Mediator
    {
        public static IServiceCollection AddMediator(this IServiceCollection services, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();

            services.AddScoped<ISender, Sender>();

            var handlerInterfaceType = typeof(IRequestHandler<,>);

            // inject services that use mediator automatically.
            var handlerTypes = assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && type.IsInterface)
                .SelectMany(type => type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                    .Select(i => new { Interface = i, implementation = type })); 

            foreach (var handler in handlerTypes)
            {
                services.AddScoped(handler.Interface, handler.implementation); 
            }

            return services; 
        }
    }


    // builder.Services.AddMediator(); 
}
