using Identity.Application.Authorization;
using Identity.Application.Features.Roles.Commands.CreateRole;
using Identity.Application.Features.Roles.Commands.DeleteRole;
using Identity.Application.Features.Roles.Commands.UpdateRole;
using Identity.Application.Features.Roles.Queries.GetRoleById;
using Identity.Application.Features.Roles.Queries.GetRoles;
using Identity.Presentation.Requests.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Mediator;

namespace Identity.Presentation.Controllers
{
    /// <summary> Role administration. Permissions are chosen by code and replaced as a whole on update. </summary>
    [ApiController]
    [Route("api/identity/roles")]
    public sealed class RolesController : ControllerBase
    {
        private readonly ISender _sender;

        public RolesController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Lists every role with summary data.
        /// </summary>
        /// <remarks>
        /// Not paginated: the number of roles in an organization is small. IsSystemRole marks the Admin role, which is
        /// managed by the system and cannot be modified or deleted.
        /// </remarks>
        /// <response code="200">Each role with its name, description, IsSystemRole, number of permissions and number of users.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the roles read permission.</response>
        [HttpGet]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GetRolesResponse>> GetRoles(CancellationToken cancellationToken)
        {
            GetRolesResponse result = await _sender.Send(new GetRolesQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns one role with its permissions grouped by module.
        /// </summary>
        /// <param name="id">The id of the role.</param>
        /// <response code="200">The role's name, description, IsSystemRole and its permissions grouped by module.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the roles read permission.</response>
        /// <response code="404">No role with that id exists.</response>
        [HttpGet("{id:guid}")]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetRoleByIdResponse>> GetRoleById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            GetRoleByIdResponse result = await _sender.Send(new GetRoleByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Creates a role with a name, a description and a set of permissions.
        /// </summary>
        /// <remarks>
        /// Permissions are chosen by code (see GET api/identity/permissions). The list may be empty, for a role
        /// without permissions, but it must be sent. Duplicated codes are ignored. The name must be unique.
        /// </remarks>
        /// <param name="request">The role's name, description and permission codes.</param>
        /// <response code="200">The created role with its id and its permission codes in order.</response>
        /// <response code="400">The name or description is empty or too long, or the permission list is missing or invalid.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the roles create permission.</response>
        /// <response code="422">A role with that name already exists, or one or more permission codes do not exist.</response>
        [HttpPost]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CreateRoleResponse>> CreateRole(
            [FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateRoleCommand(request.RoleName, request.RoleDescription, request.PermissionCodes);
            CreateRoleResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Replaces the role's name, description and permissions with the values sent.
        /// </summary>
        /// <remarks>
        /// The body is the desired final state, not a patch: permissions left out of the list are removed from the
        /// role. Users who hold the role get the new permissions on their next token refresh (at most one access
        /// token lifetime); their sessions are not closed. The Admin role cannot be modified.
        /// </remarks>
        /// <param name="id">The id of the role.</param>
        /// <param name="request">The role's new name, description and complete list of permission codes.</param>
        /// <response code="204">The role was updated.</response>
        /// <response code="400">The name or description is empty or too long, or the permission list is missing or invalid.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the roles update permission.</response>
        /// <response code="404">No role with that id exists.</response>
        /// <response code="422">It is the Admin role, another role has that name, or a permission code does not exist.</response>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Update)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateRole(
            [FromRoute] Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
        {
            var command = new UpdateRoleCommand(id, request.RoleName, request.RoleDescription, request.PermissionCodes);
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Deletes a role that no user holds.
        /// </summary>
        /// <remarks>
        /// A role still assigned to users is never revoked from them silently: the request fails with 422 and the
        /// role must first be removed from those users (PUT api/identity/users/{id}/roles). The Admin role cannot
        /// be deleted.
        /// </remarks>
        /// <param name="id">The id of the role.</param>
        /// <response code="204">The role was deleted.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the roles delete permission.</response>
        /// <response code="404">No role with that id exists.</response>
        /// <response code="422">It is the Admin role, or the role is still assigned to one or more users.</response>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DeleteRole([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new DeleteRoleCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
