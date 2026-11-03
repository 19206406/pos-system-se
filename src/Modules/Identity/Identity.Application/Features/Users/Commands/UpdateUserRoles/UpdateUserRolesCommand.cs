using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.UpdateUserRoles
{
    /// <param name="RoleIds">
    /// The complete set of roles the user must end up with: missing ones are assigned, absent ones revoked.
    /// An empty list revokes every role.
    /// </param>
    public sealed record UpdateUserRolesCommand(Guid UserId, IReadOnlyList<Guid> RoleIds) : ICommand;
}
