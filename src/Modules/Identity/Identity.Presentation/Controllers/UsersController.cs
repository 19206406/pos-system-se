using Identity.Application.Features.Users.Commands.CreateUser;
using Identity.Application.Features.Users.Commands.DeactivateUser;
using Identity.Application.Features.Users.Commands.ReactivateUser;
using Identity.Application.Features.Users.Commands.RevokeUserSessions;
using Identity.Application.Features.Users.Queries.GetUserById;
using Identity.Application.Features.Users.Queries.GetUsers;
using Identity.Application.Features.Users.Queries.GetUserSessions;
using Identity.Presentation.Extensions;
using Identity.Presentation.Requests.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants.Claims;
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
        [Authorize(Policy = PermissionCodes.Users.Read)]
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
        [Authorize(Policy = PermissionCodes.Users.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetUserByIdResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery(id);
            GetUserByIdResponse result = await _sender.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = PermissionCodes.Users.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CreateUserResponse>> CreateUser
            ([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateUserCommand(request.FullName, request.Email, request.PhoneNumber, request.Position);
            CreateUserResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:guid}/deactivate")]
        [Authorize(Policy = PermissionCodes.Users.Update)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DeactivateUser([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new DeactivateUserCommand(id, User.GetUserId()), cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:guid}/reactivate")]
        [Authorize(Policy = PermissionCodes.Users.Update)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReactivateUser([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new ReactivateUserCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:guid}/sessions")]
        [Authorize(Policy = PermissionCodes.Sessions.Read)]
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
        [Authorize(Policy = PermissionCodes.Sessions.Revoke)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeUserSessions([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new RevokeUserSessionsCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
