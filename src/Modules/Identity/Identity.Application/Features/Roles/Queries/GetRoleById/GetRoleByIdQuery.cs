using Shared.CQRS;

namespace Identity.Application.Features.Roles.Queries.GetRoleById
{
    public sealed record GetRoleByIdQuery(Guid RoleId) : IQuery<GetRoleByIdResponse>;
}
