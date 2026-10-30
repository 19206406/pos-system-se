using Identity.Presentation.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Constants.Claims;
using System.Threading.RateLimiting;

namespace Identity.Presentation
{
    public static class IdentityPresentationServicesRegistration
    {
        public static IServiceCollection AddIdentityPresentationServices(this IServiceCollection services)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(IdentityPresentationServicesRegistration).Assembly);

            AddAuthentication(services);
            AddAuthorization(services);
            AddRateLimiting(services);

            return services;
        }

        private static void AddAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();
        }

        private static void AddAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // One policy per permission code; a code missing from PermissionCodes.All has no policy (500 if used)
                foreach (var code in PermissionCodes.All)
                {
                    options.AddPolicy(code, policy => policy
                        .RequireAuthenticatedUser()
                        .RequireClaim(AuthClaimTypes.Permission, code));
                }

                // Every endpoint without an attribute requires an authenticated user; public ones say [AllowAnonymous]
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
        }

        private static void AddRateLimiting(IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                    var problemDetails = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                    await problemDetails.WriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = context.HttpContext,
                        ProblemDetails = { Status = StatusCodes.Status429TooManyRequests, Title = "Too many requests." }
                    });
                };

                AddFixedWindowPerClientIp(options, IdentityRateLimitPolicies.Credentials, permitLimit: 5, window: TimeSpan.FromMinutes(1));
                AddFixedWindowPerClientIp(options, IdentityRateLimitPolicies.PasswordTokens, permitLimit: 5, window: TimeSpan.FromMinutes(15));
                AddFixedWindowPerClientIp(options, IdentityRateLimitPolicies.Refresh, permitLimit: 30, window: TimeSpan.FromMinutes(1));
            });
        }

        // Partitioned by client IP. Behind a proxy this is only correct with UseForwardedHeaders configured in the host.
        private static void AddFixedWindowPerClientIp(RateLimiterOptions options, string policyName, int permitLimit, TimeSpan window) =>
            options.AddPolicy(policyName, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = window,
                        QueueLimit = 0
                    }));
    }
}
