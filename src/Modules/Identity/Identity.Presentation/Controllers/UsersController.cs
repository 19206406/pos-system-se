using Identity.Application.DTOs.Request;
using Identity.Application.DTOs.Responses;
using Identity.Application.Features.Users.Commands.RegisterUser;
using Microsoft.AspNetCore.Mvc;
using Shared.Mediator;

namespace Identity.Presentation.Controllers
{
    [ApiController]
    [Route("api/identity/users")]
    public class UsersController : ControllerBase
    {
        private readonly ISender _sender;

        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<ActionResult<RegisterUserResponseDto>> CreateUser
            ([FromBody] RegisterUserRequestDTO request, CancellationToken cancellationToken)
        {
            var command = new RegisterUserCommand(request.FullName, request.Email, request.PhoneNumber, request.Position);
            RegisterUserResponseDto result = await _sender.Send(command, cancellationToken);
            return Ok(result); 
        }
    }
}
