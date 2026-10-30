using Identity.Domain.Entities;

namespace Identity.Application.Dtos.Authentication
{
    /// <summary> <see cref="Entity"/> still needs a UserId, or to be added to the user's PasswordTokens. </summary>
    public sealed record IssuedPasswordToken(PasswordToken Entity, string PlainText);
}
