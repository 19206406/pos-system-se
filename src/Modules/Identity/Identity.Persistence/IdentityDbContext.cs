using Identity.Application.Constants;
using Identity.Application.Contracts.Persistence;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence
{
    public class IdentityDbContext : DbContext, IUnitOfWork
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {   
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<PasswordToken> PasswordTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Identity);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// If action throws exception, Dispose makes automatic rollback.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Traducir del Galician It does not return any parameters.It is only to control when there is a transaction with an error.
        /// </returns>
        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            var strategy = Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
                await action(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }); 
        }
    }
}
