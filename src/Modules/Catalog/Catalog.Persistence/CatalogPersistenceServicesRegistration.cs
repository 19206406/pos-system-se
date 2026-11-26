using Catalog.Application.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Persistence
{
    public static class CatalogPersistenceServicesRegistration
    {
        public static IServiceCollection AddCatalogPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PosSystemDb")
                ?? throw new InvalidOperationException(
                    "The connection string 'PosSystemDb' is missing from the configuration. " +
                    "In development set it with user-secrets (see CLAUDE.md, \"Base de datos y entorno local\").");

            services.AddDbContext<CatalogDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Catalog);
                    npgsql.EnableRetryOnFailure();
                }));

            return services;
        }
    }
}
