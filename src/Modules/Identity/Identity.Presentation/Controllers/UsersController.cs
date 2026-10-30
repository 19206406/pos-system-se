using Identity.Application.Features.Users.Commands.CreateUser;
using Identity.Application.Features.Users.Queries.GetUserById;
using Identity.Presentation.Requests.Users;
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
        public async Task<ActionResult<GetUserByIdResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery(id);
            GetUserByIdResponse result = await _sender.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CreateUserResponse>> CreateUser
            ([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateUserCommand(request.FullName, request.Email, request.PhoneNumber, request.Position);
            CreateUserResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result); 
        }
    }
}
