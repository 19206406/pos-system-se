namespace Identity.DTOs.Request
{
    public record RegisterUserRequestDTO(
        string FullName,
        string PhoneNumber, 
        string JobTitle, 
        string Position, 
        string Email); 
}
