using Identity.Application.Contracts.Messaging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Identity.IntegrationTests.Infrastructure
{
    /// <summary>
    /// The real host, in the "Testing" environment (so no user-secrets or appsettings.Development.json),
    /// with generic test-only settings. Email delivery is replaced by <see cref="CapturingEmailSender"/>.
    /// </summary>
    public sealed class IdentityApiFactory : WebApplicationFactory<Program>
    {
        // Test-only value, never used outside this project
        // 64+ characters so the tests can also sign HS512 with it (the "same key, other algorithm" attack)
        public const string SigningKey = "integration-tests-only-signing-key-0123456789-abcdefghijklmnopqrstuvwxyz";

        private readonly string _connectionString;
        private readonly CapturingEmailSender _emails;
        private readonly IReadOnlyDictionary<string, string?> _overrides;

        public IdentityApiFactory(
            string connectionString, CapturingEmailSender emails, IReadOnlyDictionary<string, string?>? overrides = null)
        {
            _connectionString = connectionString;
            _emails = emails;
            _overrides = overrides ?? new Dictionary<string, string?>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:PosSystemDb"] = _connectionString,
                ["Jwt:SigningKey"] = SigningKey,
                ["IdentityLinks:FrontendBaseUrl"] = "http://frontend.test",
                ["Smtp:Host"] = "smtp.unused.test",
                ["Smtp:FromAddress"] = "no-reply@test.local",
                ["Outbox:PollingIntervalSeconds"] = "1",
                // Tests log in many times from the same address; the rate limiting test lowers these again
                ["IdentityRateLimits:CredentialsPermitLimit"] = "100000",
                ["IdentityRateLimits:PasswordTokensPermitLimit"] = "100000",
                ["IdentityRateLimits:RefreshPermitLimit"] = "100000"
            };

            foreach (var (key, value) in _overrides)
                settings[key] = value;

            // UseSetting (not ConfigureAppConfiguration) so the values are visible while services are registered
            foreach (var (key, value) in settings)
                builder.UseSetting(key, value);

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IEmailSender>();
                services.AddSingleton<IEmailSender>(_emails);

                // ProbeController: an endpoint without any authorization attribute, to test the fallback policy
                services.AddControllers().AddApplicationPart(typeof(ProbeController).Assembly);
            });
        }
    }
}
