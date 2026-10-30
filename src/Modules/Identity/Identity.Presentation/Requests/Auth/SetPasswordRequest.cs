namespace Identity.Presentation.Requests.Auth
{
    public sealed record SetPasswordRequest(string Token, string NewPassword);
}
