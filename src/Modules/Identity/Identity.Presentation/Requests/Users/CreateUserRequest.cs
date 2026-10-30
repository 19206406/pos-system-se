namespace Identity.Presentation.Requests.Users
{
    public record CreateUserRequest(
        string FullName,
        string PhoneNumber, 
        string JobTitle, 
        string Position, 
        string Email); 
}
