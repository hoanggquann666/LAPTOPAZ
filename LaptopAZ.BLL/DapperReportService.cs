using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using LaptopAZ.DTO;

namespace LaptopAZ.BLL
{
    /// <summary>
    /// Service báo cáo sử dụng Dapper để tối ưu các truy vấn thống kê phức tạp.
    /// EF vẫn được dùng cho CRUD, Dapper chỉ dùng cho reporting/aggregation.
    /// </summary>
    public class DapperReportService
    {
        private readonly string _connectionString;

        public DapperReportService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["LaptopAZDbContext"].ConnectionString;
        }

        /// <summary>
        /// Thống kê doanh thu theo ngày trong tháng (Dapper).
        /// </summary>
        public List<DapperRevenueDTO> GetRevenueByDay(int year, int month)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        CONVERT(VARCHAR(5), OrderDate, 103) AS Period,
                        SUM(FinalAmount) AS Revenue
                    FROM Orders
                    WHERE Status IN ('Paid', 'Completed')
                      AND YEAR(OrderDate) = @Year
                      AND MONTH(OrderDate) = @Month
                    GROUP BY CONVERT(VARCHAR(5), OrderDate, 103), DAY(OrderDate)
                    ORDER BY DAY(OrderDate)";
                return conn.Query<DapperRevenueDTO>(sql, new { Year = year, Month = month }).ToList();
            }
        }

        /// <summary>
        /// Thống kê doanh thu theo tháng trong năm (Dapper).
        /// </summary>
        public List<DapperRevenueDTO> GetRevenueByMonth(int year)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        'Thg ' + CAST(MONTH(OrderDate) AS VARCHAR) AS Period,
                        SUM(FinalAmount) AS Revenue
                    FROM Orders
                    WHERE Status IN ('Paid', 'Completed')
                      AND YEAR(OrderDate) = @Year
                    GROUP BY MONTH(OrderDate)
                    ORDER BY MONTH(OrderDate)";
                return conn.Query<DapperRevenueDTO>(sql, new { Year = year }).ToList();
            }
        }

        /// <summary>
        /// Thống kê doanh thu theo năm (Dapper).
        /// </summary>
        public List<DapperRevenueDTO> GetRevenueByYear()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        CAST(YEAR(OrderDate) AS VARCHAR) AS Period,
                        SUM(FinalAmount) AS Revenue
                    FROM Orders
                    WHERE Status IN ('Paid', 'Completed')
                    GROUP BY YEAR(OrderDate)
                    ORDER BY YEAR(OrderDate)";
                return conn.Query<DapperRevenueDTO>(sql).ToList();
            }
        }

        /// <summary>
        /// Top sản phẩm bán chạy (Dapper).
        /// </summary>
        public List<DapperTopProductDTO> GetTopSellingProducts(int top = 10)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT TOP (@Top)
                        p.ProductName,
                        SUM(od.Quantity) AS QuantitySold,
                        SUM(od.Quantity * od.UnitPrice) AS TotalRevenue
                    FROM OrderDetails od
                    INNER JOIN Products p ON od.ProductId = p.ProductId
                    INNER JOIN Orders o ON od.OrderId = o.OrderId
                    WHERE o.Status IN ('Paid', 'Completed')
                    GROUP BY p.ProductName
                    ORDER BY QuantitySold DESC";
                return conn.Query<DapperTopProductDTO>(sql, new { Top = top }).ToList();
            }
        }

        /// <summary>
        /// Đếm đơn theo Status — phục vụ KPI Dashboard (aggregate, không thay EF CRUD).
        /// </summary>
        public List<OrderStatusCountDTO> GetOrderCountByStatus()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT Status, COUNT(*) AS [Count]
                    FROM Orders
                    GROUP BY Status";
                return conn.Query<OrderStatusCountDTO>(sql).ToList();
            }
        }

        /// <summary>
        /// Tổng doanh thu hôm nay (Dapper - nhanh hơn EF cho aggregate).
        /// </summary>
        public decimal GetTodayRevenue()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT ISNULL(SUM(FinalAmount), 0)
                    FROM Orders
                    WHERE Status IN ('Paid', 'Completed')
                      AND CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)";
                return conn.ExecuteScalar<decimal>(sql);
            }
        }
    }
}
