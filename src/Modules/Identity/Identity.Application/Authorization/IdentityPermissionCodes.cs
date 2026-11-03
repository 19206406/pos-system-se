namespace Identity.Application.Authorization
{
    /// <summary>
    /// Permission codes of the Identity module, used in [Authorize(Policy = ...)]. Every code must also be declared
    /// in <see cref="IdentityPermissionCatalog"/>: that is what creates its policy and its row in the permissions table.
    /// A code used in an attribute but missing from the catalog has no policy, and the request fails with a 500.
    /// </summary>
    public static class IdentityPermissionCodes
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

        public static class Roles
        {
            public const string Read = "roles.read";
            public const string Create = "roles.create";
            public const string Update = "roles.update";
            public const string Delete = "roles.delete";

            /// <summary> Assign and revoke roles of users. Separate from the rest: it can grant any permission. </summary>
            public const string Assign = "roles.assign";
        }
    }
}
