using Identity.Presentation.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Constants.Claims;
using System.Threading.RateLimiting;

namespace Identity.Presentation
{
    public static class IdentityPresentationServicesRegistration
    {
        public static IServiceCollection AddIdentityPresentationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(IdentityPresentationServicesRegistration).Assembly);

            AddAuthentication(services);
            AddAuthorization(services);
            AddRateLimiting(services, configuration);

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

        private static void AddRateLimiting(IServiceCollection services, IConfiguration configuration)
        {
            var limits = configuration.GetSection(IdentityRateLimitOptions.SectionName).Get<IdentityRateLimitOptions>() ?? new();

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

                AddFixedWindowPerClientIp(options, IdentityRateLimitPolicies.Credentials, permitLimit: limits.CredentialsPermitLimit, window: TimeSpan.FromSeconds(limits.CredentialsWindowSeconds));
                AddFixedWindowPerClientIp(options, IdentityRateLimitPolicies.PasswordTokens, permitLimit: limits.PasswordTokensPermitLimit, window: TimeSpan.FromSeconds(limits.PasswordTokensWindowSeconds));
                AddFixedWindowPerClientIp(options, IdentityRateLimitPolicies.Refresh, permitLimit: limits.RefreshPermitLimit, window: TimeSpan.FromSeconds(limits.RefreshWindowSeconds));
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
