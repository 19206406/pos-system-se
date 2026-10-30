namespace Identity.Application.Dtos.Responses
{
    public record RegisterUserResponseDto(
        Guid Id, string FullName, string JobTitle, 
        string PhoneNumber, string Email, DateTime CreatedAt); 
}
