using Shared.CQRS;

namespace Identity.Application.Features.Permissions.Queries.GetPermissions
{
    // No validator: there is nothing to validate
    public sealed record GetPermissionsQuery : IQuery<GetPermissionsResponse>;
}
