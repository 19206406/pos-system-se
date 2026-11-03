using Identity.Application.Dtos.Permissions;

namespace Identity.Application.Contracts.Persistence
{
    /// <summary> Read-only access to the permissions table: no tracking, projections only. </summary>
    public interface IPermissionReadRepository
    {
        Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken cancellationToken);

        /// <summary> Permissions whose module column equals <paramref name="moduleCode"/>, ordered by code. </summary>
        Task<IReadOnlyList<PermissionDto>> GetByModuleAsync(string moduleCode, CancellationToken cancellationToken);
    }
}
