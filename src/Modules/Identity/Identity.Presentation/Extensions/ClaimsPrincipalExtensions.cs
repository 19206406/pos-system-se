using Shared.Constants.Claims;
using System.Security.Claims;

namespace Identity.Presentation.Extensions
{
    /// <summary>
    /// Reads the authenticated user's identifiers from the access token. With inbound claim mapping
    /// disabled the claims keep their original names ("sub", "sid").
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal) =>
            ReadGuidClaim(principal, AuthClaimTypes.UserId);

        public static Guid GetSessionId(this ClaimsPrincipal principal) =>
            ReadGuidClaim(principal, AuthClaimTypes.SessionId);

        // A token we signed always carries both claims; a missing one is an issuing bug (500), not a client error
        private static Guid ReadGuidClaim(ClaimsPrincipal principal, string claimType) =>
            Guid.TryParse(principal.FindFirstValue(claimType), out var id)
                ? id
                : throw new InvalidOperationException($"The claim '{claimType}' is missing or is not a valid GUID.");
    }
}
