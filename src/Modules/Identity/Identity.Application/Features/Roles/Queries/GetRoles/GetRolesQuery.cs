using Shared.CQRS;

namespace Identity.Application.Features.Roles.Queries.GetRoles
{
    // Not paged: an organization has a handful of roles. No validator: there is nothing to validate.
    public sealed record GetRolesQuery : IQuery<GetRolesResponse>;
}
