using System;

namespace LaptopAZ.Helpers
{
    /// <summary>
    /// Centralized role checks — tránh hardcode rải rác trong UI.
    /// </summary>
    public static class RolePermissions
    {
        public const string Admin = "Admin";
        public const string WarehouseStaff = "WarehouseStaff";
        public const string SalesStaff = "SalesStaff";
        public const string Accountant = "Accountant";

        public static string Current => SessionHelper.CurrentRole ?? SalesStaff;

        public static bool IsAdmin => Current == Admin;
        public static bool IsWarehouseStaff => Current == WarehouseStaff;
        public static bool IsSalesStaff => Current == SalesStaff;
        public static bool IsAccountant => Current == Accountant;

        /// <summary>Kế toán chỉ xem, không thao tác CRUD.</summary>
        public static bool IsViewOnly => IsAccountant;

        /// <summary>Admin không được thêm/sửa/xóa sản phẩm (vẫn xem & serial).</summary>
        public static bool CanManageProducts => !IsAdmin && !IsAccountant;

        /// <summary>Admin được thêm/xóa hãng & danh mục.</summary>
        public static bool CanManageCategoriesAndBrands => !IsAccountant;

        /// <summary>Nút +New Inventory: nhân viên kho dùng; Admin thấy nhưng disabled.</summary>
        public static bool CanUseNewInventory => IsWarehouseStaff;

        public static bool CanAccessTab(string tabName)
        {
            switch (tabName)
            {
                case "Dashboard":
                    return IsAdmin || IsAccountant;
                case "Products":
                case "Categories":
                    return IsAdmin || IsWarehouseStaff;
                case "Import":
                    return IsAdmin || IsWarehouseStaff || IsAccountant;
                case "Sales":
                case "Orders":
                case "Returns":
                    return IsAdmin || IsSalesStaff || IsAccountant;
                case "Partners":
                    return IsAdmin || IsSalesStaff || IsWarehouseStaff;
                case "Staff":
                    return IsAdmin;
                default:
                    return false;
            }
        }

        public static void EnsureCanManageProducts()
        {
            if (!CanManageProducts)
                throw new UnauthorizedAccessException("Tài khoản không có quyền thêm/sửa/xóa sản phẩm.");
        }

        public static void EnsureCanMutateBusinessData()
        {
            if (IsViewOnly)
                throw new UnauthorizedAccessException("Kế toán chỉ được xem dữ liệu, không thao tác thay đổi.");
        }
    }
}
