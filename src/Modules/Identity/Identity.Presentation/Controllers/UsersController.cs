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

        /// <summary>
        /// Lists users page by page, with an optional text search and an active/inactive filter.
        /// </summary>
        /// <remarks>
        /// Search is case insensitive and matches any part of the full name or the email; "%" and "_" are taken
        /// literally. Without IsActive both active and inactive users are listed. Results are ordered by full name.
        /// Page starts at 1 and defaults to 1; PageSize defaults to 20 and goes from 1 to 100.
        /// </remarks>
        /// <param name="request">Search text, active filter, page number and page size, all optional.</param>
        /// <response code="200">The users of the page, plus the page, page size, total count and total pages.</response>
        /// <response code="400">Page is lower than 1, PageSize is out of range or the search text is too long.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the users read permission.</response>
        [HttpGet]
        [Authorize(Policy = IdentityPermissionCodes.Users.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GetUsersResponse>> GetUsers(
            [FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
        {
            var query = new GetUsersQuery(request.Search, request.IsActive, request.Page, request.PageSize);
            GetUsersResponse result = await _sender.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns the profile of one user.
        /// </summary>
        /// <remarks>
        /// Does not include roles or sessions; use GET {id}/roles and GET {id}/sessions for those.
        /// </remarks>
        /// <param name="id">The id of the user.</param>
        /// <response code="200">The user's name, email, phone number, job title, IsActive and creation date.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the users read permission.</response>
        /// <response code="404">No user with that id exists.</response>
        [HttpGet("{id:guid}")]
        [Authorize(Policy = IdentityPermissionCodes.Users.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetUserByIdResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery(id);
            GetUserByIdResponse result = await _sender.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Creates a user without a password and sends them an invite email to set it.
        /// </summary>
        /// <remarks>
        /// The administrator never chooses the password: the user sets it from the invite link
        /// (POST api/identity/auth/set-password). Until then the user cannot log in. The email is stored normalized
        /// and must be unique. The invite is recorded in the same transaction as the user and is delivered later by
        /// the outbox processor, so the response does not wait for the email. The user is created without roles;
        /// assign them with PUT {id}/roles.
        /// </remarks>
        /// <param name="request">The new user's full name, email, phone number and job title.</param>
        /// <response code="200">The created user with their id and creation date.</response>
        /// <response code="400">A field is empty, too long, or the email is not valid.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the users create permission.</response>
        /// <response code="422">A user with that email already exists.</response>
        [HttpPost]
        [Authorize(Policy = IdentityPermissionCodes.Users.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CreateUserResponse>> CreateUser
            ([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateUserCommand(request.FullName, request.Email, request.PhoneNumber, request.JobTitle);
            CreateUserResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Sends a new invite email to a user who has not set their password yet.
        /// </summary>
        /// <remarks>
        /// For when the first invite expired or was lost. When the new invite is delivered, earlier invite links stop
        /// working. A user who already has a password must use forgot-password instead.
        /// </remarks>
        /// <param name="id">The id of the user.</param>
        /// <response code="204">The new invite was queued for delivery.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the users create permission.</response>
        /// <response code="404">No user with that id exists.</response>
        /// <response code="422">The user already has a password.</response>
        [HttpPost("{id:guid}/resend-invite")]
        [Authorize(Policy = IdentityPermissionCodes.Users.Create)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ResendInvite([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new ResendInviteCommand(id), cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Deactivates a user's account and closes all their sessions.
        /// </summary>
        /// <remarks>
        /// A deactivated user cannot log in or refresh tokens; their data and roles are kept, so the account can be
        /// reactivated later. Access tokens already issued stay valid until they expire. Idempotent: deactivating an
        /// inactive user returns 204 again. An administrator cannot deactivate their own account.
        /// </remarks>
        /// <param name="id">The id of the user to deactivate.</param>
        /// <response code="204">The user is inactive and has no open sessions.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the users update permission.</response>
        /// <response code="404">No user with that id exists.</response>
        /// <response code="422">The user tried to deactivate their own account.</response>
        [HttpPost("{id:guid}/deactivate")]
        [Authorize(Policy = IdentityPermissionCodes.Users.Update)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DeactivateUser([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new DeactivateUserCommand(id, User.GetUserId()), cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Reactivates a deactivated user's account.
        /// </summary>
        /// <remarks>
        /// The user keeps their password and roles and can log in again; no session is created and no email is sent.
        /// Idempotent: reactivating an active user returns 204 again.
        /// </remarks>
        /// <param name="id">The id of the user to reactivate.</param>
        /// <response code="204">The user is active.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the users update permission.</response>
        /// <response code="404">No user with that id exists.</response>
        [HttpPost("{id:guid}/reactivate")]
        [Authorize(Policy = IdentityPermissionCodes.Users.Update)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReactivateUser([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new ReactivateUserCommand(id), cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Lists the active sessions of a user.
        /// </summary>
        /// <remarks>
        /// Only sessions that are neither revoked nor expired are listed. A user without sessions returns an empty
        /// list; a user that does not exist returns 404.
        /// </remarks>
        /// <param name="id">The id of the user.</param>
        /// <response code="200">Each session with its device, IP, creation and expiration date.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the sessions read permission.</response>
        /// <response code="404">No user with that id exists.</response>
        [HttpGet("{id:guid}/sessions")]
        [Authorize(Policy = IdentityPermissionCodes.Sessions.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetUserSessionsResponse>> GetUserSessions(
            [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            GetUserSessionsResponse result = await _sender.Send(new GetUserSessionsQuery(id), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Closes every session of a user without deactivating the account.
        /// </summary>
        /// <remarks>
        /// For a lost or stolen device: the user must log in again but can still do so. Access tokens already issued
        /// stay valid until they expire. Combine it with PUT {id}/roles when a role revocation must apply at once.
        /// </remarks>
        /// <param name="id">The id of the user.</param>
        /// <response code="204">Every session of the user was closed.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the sessions revoke permission.</response>
        /// <response code="404">No user with that id exists.</response>
        [HttpDelete("{id:guid}/sessions")]
        [Authorize(Policy = IdentityPermissionCodes.Sessions.Revoke)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeUserSessions([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new RevokeUserSessionsCommand(id), cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Lists the roles assigned to a user.
        /// </summary>
        /// <remarks>
        /// A user without roles returns an empty list; a user that does not exist returns 404.
        /// </remarks>
        /// <param name="id">The id of the user.</param>
        /// <response code="200">The user's id and their roles (id and name).</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the users read permission.</response>
        /// <response code="404">No user with that id exists.</response>
        [HttpGet("{id:guid}/roles")]
        [Authorize(Policy = IdentityPermissionCodes.Users.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetUserRolesResponse>> GetUserRoles([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            GetUserRolesResponse result = await _sender.Send(new GetUserRolesQuery(id), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Replaces the roles of a user with the set sent: assigns the missing ones and revokes the rest.
        /// </summary>
        /// <remarks>
        /// The body is the desired final state: an empty list revokes every role. Duplicated ids are ignored.
        /// The change reaches the user on their next token refresh (at most one access token lifetime); if a
        /// revocation must apply at once, also close their sessions with DELETE {id}/sessions.
        /// The Admin role cannot be revoked from the last user who holds it.
        /// </remarks>
        /// <param name="id">The id of the user.</param>
        /// <param name="request">The complete list of role ids the user must end up with.</param>
        /// <response code="204">The user's roles were replaced.</response>
        /// <response code="400">The list is missing, has more than 100 ids or contains an empty id.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the roles assign permission.</response>
        /// <response code="404">No user with that id exists.</response>
        /// <response code="422">A role does not exist, or it would leave the system without an administrator.</response>
        [HttpPut("{id:guid}/roles")]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Assign)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
