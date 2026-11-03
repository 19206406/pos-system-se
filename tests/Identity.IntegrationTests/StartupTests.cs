using Identity.IntegrationTests.Infrastructure;

namespace Identity.IntegrationTests
{
    /// <summary> Spec section 10, "Arranque": without a valid signing key the application does not start. </summary>
    public sealed class StartupTests(PostgresFixture fixture)
    {
        [Theory]
        [InlineData("")]
        [InlineData("only-31-characters-long-key-xyz")]
        public async Task The_host_does_not_start_with_an_empty_or_short_signing_key(string signingKey)
        {
            Assert.Equal(signingKey == "" ? 0 : 31, signingKey.Length);
            await using var api = fixture.CreateApi(new Dictionary<string, string?> { ["Jwt:SigningKey"] = signingKey });

            var exception = Assert.ThrowsAny<Exception>(() => api.CreateClient());

            Assert.Contains("SigningKey", Flatten(exception));
        }

        private static string Flatten(Exception exception) =>
            exception is AggregateException aggregate
                ? string.Join(" | ", aggregate.Flatten().InnerExceptions.Select(Flatten))
                : exception.Message + (exception.InnerException is null ? "" : " | " + Flatten(exception.InnerException));
    }
}
