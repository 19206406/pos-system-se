using Identity.Application.Dtos.Responses;
using Identity.Application.DTOs.Request;
using Identity.Application.DTOs.Responses;
using Identity.Application.Features.Users.Commands.RegisterUser;
using Identity.Application.Features.Users.Querys.GetUserById;
using Microsoft.AspNetCore.Http;
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

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetUserResponseDto>> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery(id);
            GetUserResponseDto result = await _sender.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterUserResponseDto>> CreateUser
            ([FromBody] RegisterUserRequestDTO request, CancellationToken cancellationToken)
        {
            var command = new RegisterUserCommand(request.FullName, request.Email, request.PhoneNumber, request.Position);
            RegisterUserResponseDto result = await _sender.Send(command, cancellationToken);
            return Ok(result); 
        }
    }
}
