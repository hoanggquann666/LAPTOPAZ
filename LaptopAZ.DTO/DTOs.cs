using System;
using System.Collections.Generic;

namespace LaptopAZ.DTO
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public string CPU { get; set; }
        public string RAM { get; set; }
        public string GPU { get; set; }
        public string Storage { get; set; }
        public string ScreenSize { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int QuantityInStock { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class CategoryDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
    }

    public class BrandDTO
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int ProductCount { get; set; }
    }

    public class SupplierDTO
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }

    public class CustomerDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductItemDTO
    {
        public string SerialNumber { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ImportDetailId { get; set; }
        public int? OrderDetailId { get; set; }
        public string Status { get; set; }
    }

    public class OrderDTO
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public int CreatedBy { get; set; }
        public string EmployeeName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Status { get; set; }
    }

    public class OrderDetailDTO
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
        public List<string> SerialNumbers { get; set; } = new List<string>();
    }

    public class ImportReceiptDTO
    {
        public int ImportReceiptId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int CreatedBy { get; set; }
        public string EmployeeName { get; set; }
        public DateTime ImportDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ImportReceiptDetailDTO
    {
        public int ImportDetailId { get; set; }
        public int ImportReceiptId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal Total => Quantity * ImportPrice;
        public List<string> SerialNumbers { get; set; } = new List<string>();
    }

    public class ReturnDTO
    {
        public int ReturnId { get; set; }
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string CustomerName { get; set; }
        public int CreatedBy { get; set; }
        public string EmployeeName { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Reason { get; set; }
        public List<string> ReturnedSerials { get; set; } = new List<string>();
    }

    public class InventoryLogDTO
    {
        public int InventoryLogId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ChangeType { get; set; }
        public int QuantityChanged { get; set; }
        public int ReferenceId { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardStatsDTO
    {
        public decimal RevenueToday { get; set; }
        public int OrdersCountToday { get; set; }
        public int LowStockCount { get; set; }
        public int ProductsCount { get; set; }
        public List<BestSellerProductDTO> BestSellers { get; set; } = new List<BestSellerProductDTO>();
        public List<LowStockProductDTO> LowStockAlerts { get; set; } = new List<LowStockProductDTO>();
        public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new Dictionary<string, decimal>();
    }

    public class BestSellerProductDTO
    {
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class LowStockProductDTO
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int QuantityInStock { get; set; }
    }
}
