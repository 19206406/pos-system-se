namespace Identity.Application.Contracts.Security
{
    public interface ITokenHasher
    {
        public string Hash(string plainText);
        public bool Verify(string plainText, string expectedHash); 
    }
}
