using Identity.DTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public UsersController()
        {
            
        }

        [HttpPost]
        public Task<IActionResult> RegisterUser([FromBody] RegisterUserRequestDTO, CancellationToken cancellationToken)
        {

        }
    }
}
