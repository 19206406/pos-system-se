using Identity.Application.Features.Roles.Commands.CreateRole;
using Identity.Application.Features.Users.Commands.CreateUser;
using Identity.Presentation.Requests.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants.Claims;
using Shared.Mediator;

namespace Identity.Presentation.Controllers
{
    [ApiController]
    public sealed class RolesController : ControllerBase
    {
        private readonly ISender _sender;

        public RolesController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        [Authorize(Policy = PermissionCodes.Users.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CreateUserResponse>> CreateRole
            ([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateRoleCommand(request.RoleName, request.RoleDescription); 
            CreateRoleResponse result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
