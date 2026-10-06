using Identity.Application.Dtos.Request;
using Identity.Application.Dtos.Responses;
using Identity.Application.DTOs.Request;
using Identity.Application.DTOs.Responses;
using Identity.Application.Features.Auth.Commands.LoginUser;
using Identity.Application.Features.Auth.Commands.SetUserPassword;
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
        public async Task<ActionResult<LoginUserResponseDto>> LoginUser(
            [FromBody] LoginUserRequestDTO request, CancellationToken cancellationToken)
        {
            var command = new LoginUserCommand(request.Email, request.Password);
            LoginUserResponseDto result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPut("password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SetUserPasswordResponseDto>> SetUserPassword(
            [FromBody] SetUserPasswordRequestDto request, CancellationToken cancellationToken)
        {
            var command = new SetUserPasswordCommand(request.Password, request.VerificationPassword, request.Email);
            SetUserPasswordResponseDto result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
