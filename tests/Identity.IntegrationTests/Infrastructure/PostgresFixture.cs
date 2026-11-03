using Identity.Application.Constants;
using Identity.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(Identity.IntegrationTests.Infrastructure.PostgresFixture))]

namespace Identity.IntegrationTests.Infrastructure
{
    /// <summary>
    /// One disposable PostgreSQL 16 container for the whole test run, with generic test-only credentials.
    /// It never touches the development database. The default API shares one migrated database;
    /// tests that need an empty database create their own with <see cref="CreateMigratedDatabaseAsync"/>.
    /// </summary>
    public sealed class PostgresFixture : IAsyncLifetime
    {
        // Ryuk (Testcontainers' reaper for orphaned containers) needs an image pull this machine could not do.
        // Without it the container is removed by DisposeAsync; if the test process is killed, remove it by hand.
        static PostgresFixture() => Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");

        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("pos_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        public CapturingEmailSender Emails { get; } = new();

        private IdentityApiFactory? _api;

        public IdentityApiFactory Api => _api ?? throw new InvalidOperationException("The fixture has not been initialized.");

        public string ConnectionString => _container.GetConnectionString();

        public async ValueTask InitializeAsync()
        {
            await _container.StartAsync();
            await MigrateAsync(ConnectionString);
            _api = new IdentityApiFactory(ConnectionString, Emails);
            await TestUsers.SynchronizePermissionsAsync(_api);
        }

        public async ValueTask DisposeAsync()
        {
            if (_api is not null)
                await _api.DisposeAsync();

            await _container.DisposeAsync();
        }

        public IdentityApiFactory CreateApi(IReadOnlyDictionary<string, string?> overrides, string? connectionString = null) =>
            new(connectionString ?? ConnectionString, Emails, overrides);

        public async Task<string> CreateMigratedDatabaseAsync(string databaseName)
        {
            await using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
                await command.ExecuteNonQueryAsync();
            }

            var connectionString = new NpgsqlConnectionStringBuilder(ConnectionString) { Database = databaseName }.ConnectionString;
            await MigrateAsync(connectionString);
            return connectionString;
        }

        // Migrations run before the host starts, so its background services find the schema ready
        private static async Task MigrateAsync(string connectionString)
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Identity))
                .Options;

            await using var context = new IdentityDbContext(options);
            await context.Database.MigrateAsync();
        }
    }
}
