using Identity.Application.Authorization;
using Identity.Application.Features.Permissions.Queries.GetModulePermissions;
using Identity.Application.Features.Permissions.Queries.GetPermissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Mediator;

namespace Identity.Presentation.Controllers
{
    /// <summary>
    /// Read-only catalog of permissions, grouped by module. Permissions are declared in code by each module and
    /// synchronized at startup, so there is no create, update or delete.
    /// </summary>
    [ApiController]
    [Route("api/identity/permissions")]
    public sealed class PermissionsController : ControllerBase
    {
        private readonly ISender _sender;

        public PermissionsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<GetPermissionsResponse>> GetPermissions(CancellationToken cancellationToken)
        {
            GetPermissionsResponse result = await _sender.Send(new GetPermissionsQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary> Permissions of one module, by its code (e.g. "identity"). </summary>
        [HttpGet("modules/{moduleCode}")]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetModulePermissionsResponse>> GetModulePermissions(
            [FromRoute] string moduleCode, CancellationToken cancellationToken)
        {
            GetModulePermissionsResponse result = await _sender.Send(new GetModulePermissionsQuery(moduleCode), cancellationToken);
            return Ok(result);
        }
    }
}
