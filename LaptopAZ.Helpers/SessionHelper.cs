using System;

namespace LaptopAZ.Helpers
{
    public static class SessionHelper
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUsername { get; set; }
        public static string CurrentFullName { get; set; }
        public static string CurrentRole { get; set; } // 'Admin', 'WarehouseStaff', 'SalesStaff'
        public static string CurrentEmail { get; set; }

        public static bool IsLoggedIn => CurrentUserId > 0;

        public static void Clear()
        {
            CurrentUserId = 0;
            CurrentUsername = null;
            CurrentFullName = null;
            CurrentRole = null;
            CurrentEmail = null;
        }
    }
}
