namespace Identity.Presentation.Requests.Auth
{
    public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}
