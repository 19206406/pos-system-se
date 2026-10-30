namespace Identity.Presentation.Requests.Auth
{
    public record SetPasswordRequest(string Password, string VerificationPassword, string Email); 
}
