using Identity.Application.Authorization;
using Identity.Application.Features.Users.Commands.CreateUser;
using Identity.Application.Features.Users.Commands.DeactivateUser;
using Identity.Application.Features.Users.Commands.ReactivateUser;
using Identity.Application.Features.Users.Commands.ResendInvite;
using Identity.Application.Features.Users.Commands.RevokeUserSessions;
using Identity.Application.Features.Users.Commands.UpdateUserRoles;
using Identity.Application.Features.Users.Queries.GetUserById;
using Identity.Application.Features.Users.Queries.GetUserRoles;
using Identity.Application.Features.Users.Queries.GetUsers;
using Identity.Application.Features.Users.Queries.GetUserSessions;
using Identity.Presentation.Extensions;
using Identity.Presentation.Requests.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Mediator;

namespace Identity.Presentation.Controllers
{
    /// <summary> User administration. Every action requires the permission policy named after its code. </summary>
    [ApiController]
    [Route("api/identity/users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly ISender _sender;

        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        [Authorize(Policy = IdentityPermissionCodes.Users.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetUsersResponse>> GetUsers(
            [FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
        {
            var query = new GetUsersQuery(request.Search, request.IsActive, request.Page, request.PageSize);
            GetUsersResponse result = await _sender.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = IdentityPermissionCodes.Users.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetUserByIdResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery(id);
            GetUserByIdResponse result = await _sender.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = IdentityPermissionCodes.Users.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CreateUserResponse>> CreateUser
            ([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateUserCommand(request.FullName, request.Email, request.PhoneNumber, request.JobTitle);
            CreateUserResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary> Issues a new invite for a user who has not set a password yet; earlier links stop working. </summary>
        [HttpPost("{id:guid}/resend-invite")]
        [Authorize(Policy = IdentityPermissionCodes.Users.Create)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ResendInvite([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new ResendInviteCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:guid}/deactivate")]
        [Authorize(Policy = IdentityPermissionCodes.Users.Update)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DeactivateUser([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new DeactivateUserCommand(id, User.GetUserId()), cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:guid}/reactivate")]
        [Authorize(Policy = IdentityPermissionCodes.Users.Update)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReactivateUser([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new ReactivateUserCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:guid}/sessions")]
        [Authorize(Policy = IdentityPermissionCodes.Sessions.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetUserSessionsResponse>> GetUserSessions(
            [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            GetUserSessionsResponse result = await _sender.Send(new GetUserSessionsQuery(id), cancellationToken);
            return Ok(result);
        }

        /// <summary> Closes every session of the user without deactivating the account. </summary>
        [HttpDelete("{id:guid}/sessions")]
        [Authorize(Policy = IdentityPermissionCodes.Sessions.Revoke)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeUserSessions([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new RevokeUserSessionsCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:guid}/roles")]
        [Authorize(Policy = IdentityPermissionCodes.Users.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetUserRolesResponse>> GetUserRoles([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            GetUserRolesResponse result = await _sender.Send(new GetUserRolesQuery(id), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Replaces the user's roles with the given set: assigns the missing ones and revokes the rest.
        /// Takes effect on the user's next refresh; the Admin role cannot be revoked from its last holder.
        /// </summary>
        [HttpPut("{id:guid}/roles")]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Assign)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateUserRoles(
            [FromRoute] Guid id, [FromBody] UpdateUserRolesRequest request, CancellationToken cancellationToken)
        {
            await _sender.Send(new UpdateUserRolesCommand(id, request.RoleIds), cancellationToken);
            return NoContent();
        }
    }
}
