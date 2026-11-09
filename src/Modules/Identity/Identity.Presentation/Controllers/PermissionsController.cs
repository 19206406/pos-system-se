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

        /// <summary>
        /// Lists every permission that can be assigned to a role, grouped by the module that declares it.
        /// </summary>
        /// <remarks>
        /// Meant for the role editor: the codes returned here are the ones accepted by POST and PUT on roles.
        /// Requires the roles read permission, because the catalog is only useful to whoever manages roles.
        /// </remarks>
        /// <response code="200">The modules, each with its code, name and permissions (code, name and description).</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the roles read permission.</response>
        [HttpGet]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GetPermissionsResponse>> GetPermissions(CancellationToken cancellationToken)
        {
            GetPermissionsResponse result = await _sender.Send(new GetPermissionsQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Lists the permissions of one module, identified by its code.
        /// </summary>
        /// <remarks>
        /// The module code is case insensitive ("Identity" finds "identity"). A module declared in code whose
        /// permissions have not been synchronized yet returns 200 with an empty list; an unknown module returns 404.
        /// </remarks>
        /// <param name="moduleCode">The module code, for example "identity".</param>
        /// <response code="200">The module's code and name, with its permissions.</response>
        /// <response code="400">The module code is empty or too long.</response>
        /// <response code="401">The request has no valid access token.</response>
        /// <response code="403">The user does not have the roles read permission.</response>
        /// <response code="404">No module with that code exists.</response>
        [HttpGet("modules/{moduleCode}")]
        [Authorize(Policy = IdentityPermissionCodes.Roles.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetModulePermissionsResponse>> GetModulePermissions(
            [FromRoute] string moduleCode, CancellationToken cancellationToken)
        {
            GetModulePermissionsResponse result = await _sender.Send(new GetModulePermissionsQuery(moduleCode), cancellationToken);
            return Ok(result);
        }
    }
}
