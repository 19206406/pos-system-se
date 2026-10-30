using Identity.Application.Features.Auth.Commands.ChangePassword;
using Identity.Application.Features.Auth.Commands.LoginUser;
using Identity.Application.Features.Auth.Commands.LogoutAllSessions;
using Identity.Application.Features.Auth.Commands.RefreshToken;
using Identity.Application.Features.Auth.Commands.RevokeMySession;
using Identity.Application.Features.Auth.Commands.SetPassword;
using Identity.Application.Features.Auth.Queries.GetCurrentUser;
using Identity.Application.Features.Auth.Queries.GetMySessions;
using Identity.Presentation.Extensions;
using Identity.Presentation.Requests.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Mediator;

namespace Identity.Presentation.Controllers
{
    /// <summary>
    /// Authentication and "my account" actions. The user's identifiers always come from the token
    /// claims ("sub", "sid"), never from the body or the route.
    /// </summary>
    [ApiController]
    [Route("api/identity/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginUserResponse>> LoginUser(
            [FromBody] LoginUserRequest request, CancellationToken cancellationToken)
        {
            var command = new LoginUserCommand(request.Email, request.Password);
            LoginUserResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPut("password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SetPasswordResponse>> SetUserPassword(
            [FromBody] SetPasswordRequest request, CancellationToken cancellationToken)
        {
            var command = new SetPasswordCommand(request.Email, request.Password, request.VerificationPassword);
            SetPasswordResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary> Exchanges a refresh token for a new token pair; the refresh token sent is invalidated. </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(
            [FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var command = new RefreshTokenCommand(request.RefreshToken, HttpContext.GetClientContext());
            RefreshTokenResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary> Closes every session of the authenticated user, including the current one. </summary>
        [HttpPost("logout-all")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAllSessions(CancellationToken cancellationToken)
        {
            await _sender.Send(new LogoutAllSessionsCommand(User.GetUserId()), cancellationToken);
            return NoContent();
        }

        /// <summary> Changes the password and closes every other session; the current one is kept. </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var command = new ChangePasswordCommand(
                User.GetUserId(), User.GetSessionId(), request.CurrentPassword, request.NewPassword);

            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary> Profile, roles and permissions of the authenticated user, read from the database. </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GetCurrentUserResponse>> GetCurrentUser(CancellationToken cancellationToken)
        {
            GetCurrentUserResponse result = await _sender.Send(new GetCurrentUserQuery(User.GetUserId()), cancellationToken);
            return Ok(result);
        }

        /// <summary> Active sessions of the authenticated user, flagging the current one. </summary>
        [HttpGet("sessions")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GetMySessionsResponse>> GetMySessions(CancellationToken cancellationToken)
        {
            var query = new GetMySessionsQuery(User.GetUserId(), User.GetSessionId());
            GetMySessionsResponse result = await _sender.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary> Closes one session of the authenticated user (for example, another device). </summary>
        [HttpDelete("sessions/{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeMySession([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new RevokeMySessionCommand(User.GetUserId(), id), cancellationToken);
            return NoContent();
        }
    }
}
