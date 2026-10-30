namespace Identity.Domain.Constants
{
    /// <summary> Roles created by the seed migration; their ids are fixed so code can reference them. </summary>
    public static class SystemRoles
    {
        public static readonly Guid AdminId = new("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11");
        public const string AdminName = "Admin";
    }
}
