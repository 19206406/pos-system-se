using Identity.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    internal sealed class PermissionRepository : IPermissionRepository
    {
        private readonly IdentityDbContext _context;

        public PermissionRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyDictionary<string, Guid>> GetIdsByCodesAsync(
            IReadOnlyCollection<string> codes, CancellationToken cancellationToken) =>
            await _context.Permissions
                .Where(p => codes.Contains(p.Identifier))
                .Select(p => new { p.Identifier, p.Id })
                .ToDictionaryAsync(p => p.Identifier, p => p.Id, StringComparer.Ordinal, cancellationToken);
    }
}
