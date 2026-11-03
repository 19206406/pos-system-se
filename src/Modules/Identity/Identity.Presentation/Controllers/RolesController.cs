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

        [HttpGet]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<GetRolesResponse>> GetRoles(CancellationToken cancellationToken)
        {
            GetRolesResponse result = await _sender.Send(new GetRolesQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetRoleByIdResponse>> GetRoleById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            GetRoleByIdResponse result = await _sender.Send(new GetRoleByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CreateRoleResponse>> CreateRole(
            [FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateRoleCommand(request.RoleName, request.RoleDescription, request.PermissionCodes);
            CreateRoleResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary> Replaces the role's name, description and permissions; the body is the desired final state. </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Update)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateRole(
            [FromRoute] Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
        {
            var command = new UpdateRoleCommand(id, request.RoleName, request.RoleDescription, request.PermissionCodes);
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary> Deletes a role that no user holds; the Admin role cannot be deleted. </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DeleteRole([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new DeleteRoleCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
