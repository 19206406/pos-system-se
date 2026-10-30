using Identity.Application.Features.Auth.Commands.LoginUser;
using Identity.Application.Features.Auth.Commands.SetPassword;
using Identity.Presentation.Requests.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Mediator;

namespace Identity.Presentation.Controllers
{
    [ApiController]
    [Route("api/identity/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("login")]
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
    }
}
