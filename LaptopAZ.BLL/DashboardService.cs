using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LaptopAZ.DTO;
using LaptopAZ.Repository;

namespace LaptopAZ.BLL
{
    public class DashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public DashboardStatsDTO GetDashboardStats()
        {
            var stats = new DashboardStatsDTO();
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            // Today's stats
            stats.RevenueToday = _unitOfWork.Orders.Query()
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow && o.Status == "Paid")
                .Select(o => (decimal?)o.FinalAmount)
                .DefaultIfEmpty(0)
                .Sum() ?? 0;

            stats.OrdersCountToday = _unitOfWork.Orders.Count(o => o.OrderDate >= today && o.OrderDate < tomorrow && o.Status == "Paid");

            // Low Stock threshold: < 3
            stats.LowStockCount = _unitOfWork.Products.Count(p => p.QuantityInStock < 3 && p.IsActive);
            stats.ProductsCount = _unitOfWork.Products.Count(p => p.IsActive);

            // Order status counts
            stats.PendingOrdersCount = _unitOfWork.Orders.Count(o => o.Status == "Pending");
            stats.CancelledOrdersCount = _unitOfWork.Orders.Count(o => o.Status == "Cancelled");
            stats.CompletedOrdersCount = _unitOfWork.Orders.Count(o => o.Status == "Completed" || o.Status == "Paid");

            // Low stock alerts list
            stats.LowStockAlerts = _unitOfWork.Products.Query()
                .Where(p => p.QuantityInStock < 3 && p.IsActive)
                .OrderBy(p => p.QuantityInStock)
                .Select(p => new LowStockProductDTO
                {
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    QuantityInStock = p.QuantityInStock
                })
                .Take(10)
                .ToList();

            // Best sellers
            stats.BestSellers = _unitOfWork.OrderDetails.Query()
                .Where(od => od.Order.Status == "Paid")
                .GroupBy(od => od.Product.ProductName)
                .Select(g => new BestSellerProductDTO
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToList();

            // Monthly revenue chart data (Last 6 months)
            DateTime sixMonthsAgo = DateTime.Today.AddMonths(-6);
            var orders = _unitOfWork.Orders.Query()
                .Where(o => o.OrderDate >= sixMonthsAgo && o.Status == "Paid")
                .ToList(); // Materialize to do clean in-memory date formatting

            var monthlyGroup = orders
                .GroupBy(o => o.OrderDate.ToString("MM/yyyy"))
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(o => o.FinalAmount)
                );

            // Populate monthly revenue dict
            for (int i = 5; i >= 0; i--)
            {
                string monthKey = DateTime.Today.AddMonths(-i).ToString("MM/yyyy");
                stats.MonthlyRevenue[monthKey] = monthlyGroup.ContainsKey(monthKey) ? monthlyGroup[monthKey] : 0;
            }

            return stats;
        }

        /// <summary>
        /// Returns daily revenue for each day of the given month/year.
        /// Keys are formatted as "dd/MM" (e.g., "01/05", "15/05").
        /// </summary>
        public Dictionary<string, decimal> GetRevenueByDay(int year, int month)
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            var orders = _unitOfWork.Orders.Query()
                .Where(o => o.OrderDate >= start && o.OrderDate < end && o.Status == "Paid")
                .ToList();

            var grouped = orders
                .GroupBy(o => o.OrderDate.Day)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.FinalAmount));

            var result = new Dictionary<string, decimal>();
            for (int d = 1; d <= daysInMonth; d++)
            {
                string key = $"{d:D2}/{month:D2}";
                result[key] = grouped.ContainsKey(d) ? grouped[d] : 0;
            }
            return result;
        }

        /// <summary>
        /// Returns monthly revenue for each month of the given year.
        /// Keys are formatted as "Thg N" (e.g., "Thg 1", "Thg 12").
        /// </summary>
        public Dictionary<string, decimal> GetRevenueByMonth(int year)
        {
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year + 1, 1, 1);

            var orders = _unitOfWork.Orders.Query()
                .Where(o => o.OrderDate >= start && o.OrderDate < end && o.Status == "Paid")
                .ToList();

            var grouped = orders
                .GroupBy(o => o.OrderDate.Month)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.FinalAmount));

            var result = new Dictionary<string, decimal>();
            for (int m = 1; m <= 12; m++)
            {
                string key = $"Thg {m}";
                result[key] = grouped.ContainsKey(m) ? grouped[m] : 0;
            }
            return result;
        }

        /// <summary>
        /// Returns yearly revenue for each year that has data.
        /// If no data, returns the last 3 years with 0.
        /// Keys are formatted as "YYYY" (e.g., "2023", "2024").
        /// </summary>
        public Dictionary<string, decimal> GetRevenueByYear()
        {
            var orders = _unitOfWork.Orders.Query()
                .Where(o => o.Status == "Paid")
                .ToList();

            var grouped = orders
                .GroupBy(o => o.OrderDate.Year)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.FinalAmount));

            int currentYear = DateTime.Today.Year;
            var result = new Dictionary<string, decimal>();

            if (!grouped.Any())
            {
                // Show last 3 years with 0 if no data
                for (int y = currentYear - 2; y <= currentYear; y++)
                    result[y.ToString()] = 0;
            }
            else
            {
                int minYear = grouped.Keys.Min();
                for (int y = minYear; y <= currentYear; y++)
                {
                    result[y.ToString()] = grouped.ContainsKey(y) ? grouped[y] : 0;
                }
            }
            return result;
        }
    }
}
