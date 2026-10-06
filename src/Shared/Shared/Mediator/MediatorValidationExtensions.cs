using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Mediator.Behaviors;
using System.Reflection;

namespace Shared.Mediator
{
    public static class MediatorValidationExtensions
    {
        public static IServiceCollection AddMediatorValidation(this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies.Distinct())
                services.AddValidatorsFromAssembly(assembly);

            // TryAddEnumerable makes this idempotent: every module can call it
            // without the behavior running more than once per request

            services.TryAddEnumerable(
                ServiceDescriptor.Scoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));

            return services; 
        }
    }
}
