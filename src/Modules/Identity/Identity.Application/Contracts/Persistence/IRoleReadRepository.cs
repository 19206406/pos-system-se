using Identity.Application.Dtos.Roles;

namespace Identity.Application.Contracts.Persistence
{
    /// <summary> Read-only access to roles: no tracking, projections only. </summary>
    public interface IRoleReadRepository
    {
        Task<IReadOnlyList<RoleSummaryDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<RoleDetailDto?> GetDetailByIdAsync(Guid roleId, CancellationToken cancellationToken);
        Task<IReadOnlyList<RoleReferenceDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
