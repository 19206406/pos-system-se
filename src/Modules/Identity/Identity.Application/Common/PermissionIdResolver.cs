using Identity.Application.Contracts.Persistence;
using Shared.Exceptions;

namespace Identity.Application.Common
{
    /// <summary> Turns the permission codes chosen for a role into ids, rejecting codes that do not exist. </summary>
    public static class PermissionIdResolver
    {
        private const int MaxCodesInMessage = 10;

        /// <returns> One id per distinct code. </returns>
        /// <exception cref="BusinessException"> If any code is not in the permissions table. </exception>
        public static async Task<IReadOnlyList<Guid>> ResolveAsync(
            IPermissionRepository permissionRepository, IReadOnlyList<string> codes, CancellationToken cancellationToken)
        {
            var distinctCodes = codes.Distinct(StringComparer.Ordinal).ToList();
            if (distinctCodes.Count == 0)
                return [];

            var idsByCode = await permissionRepository.GetIdsByCodesAsync(distinctCodes, cancellationToken);

            var unknown = distinctCodes.Where(code => !idsByCode.ContainsKey(code)).ToList();
            // The message goes to the response and the log: list a few codes, not a whole oversized payload
            if (unknown.Count > 0)
                throw new BusinessException(
                    $"{unknown.Count} permission code(s) do not exist: {string.Join(", ", unknown.Take(MaxCodesInMessage))}" +
                    (unknown.Count > MaxCodesInMessage ? ", ..." : "."));

            return distinctCodes.Select(code => idsByCode[code]).ToList();
        }
    }
}
