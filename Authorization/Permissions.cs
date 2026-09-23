namespace InventoryManagementSystem.Authorization
{
    public static class Permissions
    {
        public const string VIEW_ASSET = "VIEW_ASSET";
        public const string CREATE_ASSET = "CREATE_ASSET";
        public const string EDIT_ASSET = "EDIT_ASSET";
        public const string DELETE_ASSET = "DELETE_ASSET";
        public const string VIEW_REPORT = "VIEW_REPORT";
        public const string MANAGE_USERS = "MANAGE_USERS";
        public const string MANAGE_ROLES = "MANAGE_ROLES";
        public const string MANAGE_PERMISSIONS = "MANAGE_PERMISSIONS";
        public const string MANAGE_ORGANIZATION = "MANAGE_ORGANIZATION";
        public const string VIEW_NOTIFICATIONS = "VIEW_NOTIFICATIONS";

        public static readonly List<string> AllPermissions = new List<string>
        {
            VIEW_ASSET,
            CREATE_ASSET,
            EDIT_ASSET,
            DELETE_ASSET,
            VIEW_REPORT,
            MANAGE_USERS,
            MANAGE_ROLES,
            MANAGE_PERMISSIONS,
            MANAGE_ORGANIZATION,
            VIEW_NOTIFICATIONS
        };
    }
}
