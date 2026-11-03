using Shared.CQRS;

namespace Identity.Application.Features.Permissions.Queries.GetModulePermissions
{
    /// <param name="ModuleCode"> Code of the module, e.g. "identity". Compared case-insensitively. </param>
    public sealed record GetModulePermissionsQuery(string ModuleCode) : IQuery<GetModulePermissionsResponse>;
}
