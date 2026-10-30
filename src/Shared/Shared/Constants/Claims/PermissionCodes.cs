namespace Shared.Constants.Claims
{
    /// <summary>
    /// Every code must exist in three places: the permissions table (seed), the role that grants it
    /// and <see cref="All"/>. A code missing from <see cref="All"/> has no policy, and using it produces a 500.
    /// </summary>
    public static class PermissionCodes
    {
        public static class Users
        {
            public const string Read = "users.read";
            public const string Create = "users.create";
            public const string Update = "users.update";
        }

        public static class Sessions
        {
            public const string Read = "sessions.read";
            public const string Revoke = "sessions.revoke";
        }

        public static readonly IReadOnlyList<string> All =
        [
            Users.Read, Users.Create, Users.Update,
            Sessions.Read, Sessions.Revoke
        ];
    }
}
