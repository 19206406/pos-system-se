namespace Identity.Application.Dtos.Authentication
{
    public sealed record UserAccessDto(IReadOnlyCollection<string> Roles, IReadOnlyCollection<string> Permissions); 
}
