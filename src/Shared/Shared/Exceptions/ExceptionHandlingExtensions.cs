using Microsoft.Extensions.DependencyInjection;
using Shared.Exceptions.Handler;

namespace Shared.Exceptions
{
    public static class ExceptionHandlingExtensions
    {
        public static IServiceCollection AddSharedExceptionHandling(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                };
            });

            return services;
        }
    }
}
