using Identity.Application.Dtos.Users;

namespace Identity.Application.Contracts.Persistence
{
    /// <summary> Read-only access to users: no tracking, projections only. Never exposes HashPassword. </summary>
    public interface IUserReadRepository
    {
        Task<UserProfileDto?> GetProfileByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken);
        Task<UserPageDto> GetPagedAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken cancellationToken);
    }
}
