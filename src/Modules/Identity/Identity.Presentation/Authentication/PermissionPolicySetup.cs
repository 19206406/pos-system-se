using Identity.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Shared.Constants.Claims;

namespace Identity.Presentation.Authentication
{
    /// <summary>
    /// One authorization policy per permission code declared by any module, named after the code and requiring the
    /// "permission" claim with that value. A code used in [Authorize(Policy = ...)] that no catalog declares has no
    /// policy, and the request fails with a 500.
    /// </summary>
    internal sealed class PermissionPolicySetup : IConfigureOptions<AuthorizationOptions>
    {
        private readonly PermissionCatalog _catalog;

        public PermissionPolicySetup(PermissionCatalog catalog)
        {
            _catalog = catalog;
        }

        public void Configure(AuthorizationOptions options)
        {
            foreach (var code in _catalog.AllCodes)
            {
                options.AddPolicy(code, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AuthClaimTypes.Permission, code));
            }
        }
    }
}
