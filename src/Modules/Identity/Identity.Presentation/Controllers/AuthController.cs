using Identity.Application.Features.Auth.Commands.ChangePassword;
using Identity.Application.Features.Auth.Commands.ForgotPassword;
using Identity.Application.Features.Auth.Commands.LoginUser;
using Identity.Application.Features.Auth.Commands.LogoutAllSessions;
using Identity.Application.Features.Auth.Commands.LogoutUser;
using Identity.Application.Features.Auth.Commands.RefreshToken;
using Identity.Application.Features.Auth.Commands.RevokeMySession;
using Identity.Application.Features.Auth.Commands.SetPassword;
using Identity.Application.Features.Auth.Queries.GetCurrentUser;
using Identity.Application.Features.Auth.Queries.GetMySessions;
using Identity.Application.Features.Auth.Queries.ValidatePasswordToken;
using Identity.Presentation.Authentication;
using Identity.Presentation.Extensions;
using Identity.Presentation.Requests.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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

        /// <summary>
        /// Authenticates a user with email and password and opens a new session.
        /// </summary>
        /// <remarks>
        /// Each successful login creates its own session (one per device), tagged with the client's User-Agent and IP.
        /// Every failure cause (unknown email, wrong password, inactive account, pending invite) returns the same 401
        /// with the same message, so the endpoint cannot be used to discover accounts.
        /// Rate limited per client IP ("identity-credentials" policy).
        /// </remarks>
        /// <param name="request">The user's email and password.</param>
        /// <response code="200">The user's name and email, plus the access token and refresh token with their expirations.</response>
        /// <response code="400">The email or password is empty, malformed or too long.</response>
        /// <response code="401">The credentials are not valid. Generic message for every cause.</response>
        /// <response code="429">Too many attempts from this IP in the current window.</response>
        [HttpPost("login")]
        [EnableRateLimiting(IdentityRateLimitPolicies.Credentials)]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<LoginUserResponse>> LoginUser(
            [FromBody] LoginUserRequest request, CancellationToken cancellationToken)
        {
            var command = new LoginUserCommand(request.Email, request.Password, HttpContext.GetClientContext());
            LoginUserResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Closes the session that owns the refresh token sent (logout from the current device).
        /// </summary>
        /// <remarks>
        /// Anonymous on purpose: the client can log out even when its access token has already expired.
        /// Idempotent and silent: an empty, unknown or already revoked token also returns 204, so the response
        /// reveals nothing about the token. The access token already issued stays valid until it expires;
        /// the client must discard it.
        /// </remarks>
        /// <param name="request">The refresh token of the session to close.</param>
        /// <response code="204">Always, whether or not a session was closed.</response>
        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> LogoutUser([FromBody] LogoutUserRequest request, CancellationToken cancellationToken)
        {
            await _sender.Send(new LogoutUserCommand(request.RefreshToken ?? string.Empty), cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Requests a password reset link for the given email.
        /// </summary>
        /// <remarks>
        /// Returns the same 202 whether or not the email exists or the account is active, so it cannot be used to
        /// discover accounts. For an active user the request is recorded in the outbox; the reset token is generated
        /// and the email is sent later by the outbox processor, so the response never waits for the email.
        /// It does not close any session: someone who only knows the email must not be able to log the user out.
        /// If the user still has a pending invite, the reset link replaces it.
        /// Rate limited per client IP ("identity-password-tokens" policy).
        /// </remarks>
        /// <param name="request">The email of the account to recover.</param>
        /// <response code="202">The request was accepted. Says nothing about whether the account exists.</response>
        /// <response code="400">The email is empty, malformed or too long.</response>
        /// <response code="429">Too many requests from this IP in the current window.</response>
        [HttpPost("forgot-password")]
        [EnableRateLimiting(IdentityRateLimitPolicies.PasswordTokens)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            await _sender.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
            return Accepted();
        }

        /// <summary>
        /// Checks whether an invite or reset token can still be used, without consuming it.
        /// </summary>
        /// <remarks>
        /// Meant for the frontend to decide, before showing the password form, whether the link is still valid.
        /// An invalid token is not an error: it returns 200 with IsValid = false. A missing, expired or already used
        /// token give the same answer, and the response contains no user data.
        /// Rate limited per client IP ("identity-password-tokens" policy).
        /// </remarks>
        /// <param name="token">The token received in the invite or reset link.</param>
        /// <response code="200">IsValid, and the token type ("invite" or "reset") when it is valid.</response>
        /// <response code="400">The token is empty or too long.</response>
        /// <response code="429">Too many requests from this IP in the current window.</response>
        [HttpGet("password-token/validate")]
        [EnableRateLimiting(IdentityRateLimitPolicies.PasswordTokens)]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ValidatePasswordTokenResponse>> ValidatePasswordToken(
            [FromQuery] string token, CancellationToken cancellationToken)
        {
            ValidatePasswordTokenResponse result = await _sender.Send(new ValidatePasswordTokenQuery(token), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Sets the user's password using an invite or reset token, and closes every session of the user.
        /// </summary>
        /// <remarks>
        /// Completes both flows: the first password of an invited user and the recovery of a forgotten one.
        /// The token is single use and is consumed atomically, so two simultaneous requests with the same token
        /// cannot both succeed; every other pending token of the user is invalidated too.
        /// Every session of the user is closed and there is no automatic login: the user must log in with the new
        /// password. A missing, used or expired token, or an inactive user, give the same 400 and message.
        /// Rate limited per client IP ("identity-credentials" policy).
        /// </remarks>
        /// <param name="request">The token from the link and the new password.</param>
        /// <response code="204">The password was set and every session of the user was closed.</response>
        /// <response code="400">The new password does not meet the rules, or the link is invalid or has expired.</response>
        /// <response code="429">Too many attempts from this IP in the current window.</response>
        [HttpPost("set-password")]
        [EnableRateLimiting(IdentityRateLimitPolicies.Credentials)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> SetPassword(
            [FromBody] SetPasswordRequest request, CancellationToken cancellationToken)
        {
            await _sender.Send(new SetPasswordCommand(request.Token, request.NewPassword), cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Exchanges a refresh token for a new access token and refresh token (rotation).
        /// </summary>
        /// <remarks>
        /// The refresh token sent stops working and the new one must be used next time. The session keeps its
        /// original absolute expiration: refreshing never extends it, so the user must log in again when it ends.
        /// Roles and permissions are read again from the database, so role changes reach the new access token.
        /// Presenting a token that was already rotated is treated as theft and closes every session of the user.
        /// Every failure cause (unknown, expired, revoked or reused token, inactive user, concurrent refresh)
        /// returns the same 401 and message.
        /// Rate limited per client IP ("identity-refresh" policy).
        /// </remarks>
        /// <param name="request">The current refresh token.</param>
        /// <response code="200">The new access token and refresh token with their expirations.</response>
        /// <response code="400">The request body is malformed.</response>
        /// <response code="401">The refresh token is not valid. Generic message for every cause.</response>
        /// <response code="429">Too many requests from this IP in the current window.</response>
        [HttpPost("refresh")]
        [EnableRateLimiting(IdentityRateLimitPolicies.Refresh)]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(
            [FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var command = new RefreshTokenCommand(request.RefreshToken ?? string.Empty, HttpContext.GetClientContext());
            RefreshTokenResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Closes every session of the authenticated user on every device, including the current one.
        /// </summary>
        /// <remarks>
        /// The user is taken from the access token ("sub" claim). After this call no refresh token of the user works;
        /// access tokens already issued stay valid until they expire.
        /// </remarks>
        /// <response code="204">Every session of the user was closed.</response>
        /// <response code="401">The request has no valid access token.</response>
        [HttpPost("logout-all")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAllSessions(CancellationToken cancellationToken)
        {
            await _sender.Send(new LogoutAllSessionsCommand(User.GetUserId()), cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Changes the authenticated user's password, given the current one, and closes every other session.
        /// </summary>
        /// <remarks>
        /// The user and the current session are taken from the access token ("sub" and "sid" claims). The session
        /// making the request is kept, so the user stays logged in on this device; the others are closed.
        /// A wrong current password returns 422, not 401, so the client does not mistake it for an expired session.
        /// Rate limited per client IP ("identity-credentials" policy).
        /// </remarks>
        /// <param name="request">The current password and the new one.</param>
        /// <response code="204">The password was changed and every other session was closed.</response>
        /// <response code="400">The new password does not meet the rules or is equal to the current one.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="422">The current password is incorrect, or the account cannot change its password.</response>
        /// <response code="429">Too many attempts from this IP in the current window.</response>
        [HttpPost("change-password")]
        [EnableRateLimiting(IdentityRateLimitPolicies.Credentials)]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var command = new ChangePasswordCommand(
                User.GetUserId(), User.GetSessionId(), request.CurrentPassword, request.NewPassword);

            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Returns the profile, roles and permissions of the authenticated user.
        /// </summary>
        /// <remarks>
        /// The user is taken from the access token ("sub" claim). Roles and permissions are read from the database,
        /// not from the token, so the response reflects role changes made after the token was issued.
        /// </remarks>
        /// <response code="200">The user's profile with their role names and permission codes.</response>
        /// <response code="401">The request has no valid access token.</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GetCurrentUserResponse>> GetCurrentUser(CancellationToken cancellationToken)
        {
            GetCurrentUserResponse result = await _sender.Send(new GetCurrentUserQuery(User.GetUserId()), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Lists the active sessions of the authenticated user, flagging the one making the request.
        /// </summary>
        /// <remarks>
        /// The user and the current session are taken from the access token ("sub" and "sid" claims). Only sessions
        /// that are neither revoked nor expired are listed. Use the session id with DELETE sessions/{id} to close one.
        /// </remarks>
        /// <response code="200">Each session with its device, IP, creation and expiration date, and IsCurrent.</response>
        /// <response code="401">The request has no valid access token.</response>
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

        /// <summary>
        /// Closes one session of the authenticated user, for example on another device.
        /// </summary>
        /// <remarks>
        /// The user is taken from the access token ("sub" claim) and the session must belong to them: a session of
        /// another user, one that does not exist and one already closed all return the same 404.
        /// Closing the current session works too, and is equivalent to logging out from this device.
        /// </remarks>
        /// <param name="id">The id of the session, as returned by GET sessions.</param>
        /// <response code="204">The session was closed.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="404">The session does not exist, is not active or belongs to another user.</response>
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
