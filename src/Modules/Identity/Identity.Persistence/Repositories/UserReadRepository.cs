using Identity.Application.Contracts.Persistence;
using Identity.Application.Dtos.Users;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    internal sealed class UserReadRepository : IUserReadRepository
    {
        private const char LikeEscapeCharacter = '\\';

        private readonly IdentityDbContext _context;

        public UserReadRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto?> GetProfileByIdAsync(Guid userId, CancellationToken cancellationToken) =>
            await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new UserProfileDto(u.Id, u.FullName, u.Email, u.PhoneNumber, u.JobTitle, u.IsActive))
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken) =>
            await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);

        public async Task<UserPageDto> GetPagedAsync(
            string? search, bool? isActive, int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = _context.Users.AsNoTracking();

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{EscapeLikePattern(search.Trim())}%";
                query = query.Where(u =>
                    EF.Functions.ILike(u.FullName, pattern, LikeEscapeCharacter.ToString())
                    || EF.Functions.ILike(u.Email, pattern, LikeEscapeCharacter.ToString()));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // Tie-break by Id so pages stay stable when several users share the same name
            var items = await query
                .OrderBy(u => u.FullName).ThenBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItemDto(u.Id, u.FullName, u.Email, u.JobTitle, u.IsActive, u.CreatedAt))
                .ToListAsync(cancellationToken);

            return new UserPageDto(items, totalCount);
        }

        /// <summary> Makes '%' and '_' typed by the user literal instead of LIKE wildcards. </summary>
        private static string EscapeLikePattern(string value) =>
            value
                .Replace(LikeEscapeCharacter.ToString(), $"{LikeEscapeCharacter}{LikeEscapeCharacter}")
                .Replace("%", $"{LikeEscapeCharacter}%")
                .Replace("_", $"{LikeEscapeCharacter}_");
    }
}
