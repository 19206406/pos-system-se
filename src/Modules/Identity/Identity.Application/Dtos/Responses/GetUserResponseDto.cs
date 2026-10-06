namespace Identity.Application.Dtos.Responses
{
    public record GetUserResponseDto(
        Guid Id, string FullName, string? PhoneNumber, 
        string JobTitle, string Email, bool IsActive, DateTime CreatedAt); 
}
