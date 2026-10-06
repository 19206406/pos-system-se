namespace Identity.Application.Contracts.Security
{
    public interface ISecureTokenGenerator
    {
        public string GenerateToken(); 
    }
}
