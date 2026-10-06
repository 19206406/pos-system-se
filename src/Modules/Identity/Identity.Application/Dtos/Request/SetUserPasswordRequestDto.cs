namespace Identity.Application.Dtos.Request
{
    public record SetUserPasswordRequestDto(string Password, string VerificationPassword, string Email); 
}
