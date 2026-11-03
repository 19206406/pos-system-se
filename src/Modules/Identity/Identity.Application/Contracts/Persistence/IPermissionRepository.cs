namespace Identity.Application.Contracts.Persistence
{
    public interface IPermissionRepository
    {
        /// <summary> Id of each code in <paramref name="codes"/> that exists; codes that do not exist are absent. </summary>
        Task<IReadOnlyDictionary<string, Guid>> GetIdsByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken);
    }
}
