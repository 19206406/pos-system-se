using Identity.Application.Dtos.Authentication;

namespace Identity.Application.Contracts.Security
{
    /// <summary>
    /// Builds invite and reset tokens with their configured lifetimes. Only the SHA-256 hash is stored;
    /// the plain token is returned once so it can be delivered to the user, and is never persisted or logged.
    /// </summary>
    public interface IPasswordTokenIssuer
    {
        IssuedPasswordToken IssueInvite(DateTime nowUtc);
        IssuedPasswordToken IssueReset(DateTime nowUtc);
    }
}
