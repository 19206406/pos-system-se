namespace Shared.Constants.Claims
{
    public static class AuthClaimTypes
    {
        public const string SessionsId = "sid";
        public const string Role = "role";
        public const string Permission = "permission"; 
    }

    public static class PermissionCodes
    {
        public static class Users
        {
            public const string Read = "users.read";
            public const string Create = "users.create"; 
        }

        public static readonly IReadOnlyList<string> All = [Users.Read, Users.Create]; 
    }
}
