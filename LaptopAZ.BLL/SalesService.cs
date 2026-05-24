using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LaptopAZ.DTO;
using LaptopAZ.Models;
using LaptopAZ.Repository;

namespace LaptopAZ.BLL
{
    public class SalesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SalesService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public List<OrderDTO> GetAllOrders(string search = null)
        {
            var query = _unitOfWork.Orders.Query()
                .Include(o => o.Customer)
                .Include(o => o.User);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(o => o.OrderCode.ToLower().Contains(search) || 
                                         o.Customer.CustomerName.ToLower().Contains(search) || 
                                         o.Customer.Phone.Contains(search));
            }

            return query.OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDTO
                {
                    OrderId = o.OrderId,
                    OrderCode = o.OrderCode,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.CustomerName,
                    CustomerPhone = o.Customer.Phone,
                    CreatedBy = o.CreatedBy,
                    EmployeeName = o.User.FullName,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    DiscountAmount = o.DiscountAmount,
                    FinalAmount = o.FinalAmount,
                    Status = o.Status
                }).ToList();
        }

        public List<OrderDetailDTO> GetOrderDetails(int orderId)
        {
            return _unitOfWork.OrderDetails.Query()
                .Include(od => od.Product)
                .Include(od => od.ProductItems)
                .Where(od => od.OrderId == orderId)
                .Select(od => new OrderDetailDTO
                {
                    OrderDetailId = od.OrderDetailId,
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
                    ProductName = od.Product.ProductName,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    SerialNumbers = od.ProductItems.Select(pi => pi.SerialNumber).ToList()
                }).ToList();
        }

        /// <summary>
        /// Places an order under database transaction, binding serial numbers, updating stocks and logging inventory.
        /// </summary>
        public bool CreateOrder(int customerId, int createdBy, decimal discount, List<OrderDetailDTO> details)
        {
            if (details == null || !details.Any())
                throw new ArgumentException("Giỏ hàng rỗng, không thể tạo hóa đơn.");

            _unitOfWork.BeginTransaction();
            try
            {
                // Generate a unique Order Code
                string orderCode = "HD-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);
                while (_unitOfWork.Orders.Any(o => o.OrderCode == orderCode))
                {
                    orderCode = "HD-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);
                }

                decimal totalAmount = details.Sum(d => d.Quantity * d.UnitPrice);
                decimal finalAmount = totalAmount - discount;
                if (finalAmount < 0) finalAmount = 0;

                // 1. Create Order
                var order = new Order
                {
                    OrderCode = orderCode,
                    CustomerId = customerId,
                    CreatedBy = createdBy,
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    DiscountAmount = discount,
                    FinalAmount = finalAmount,
                    Status = "Paid"
                };
                _unitOfWork.Orders.Add(order);
                _unitOfWork.SaveChanges(); // Generate OrderId

                foreach (var det in details)
                {
                    if (det.Quantity <= 0)
                        throw new Exception("Số lượng mua phải lớn hơn 0.");
                    if (det.SerialNumbers.Count != det.Quantity)
                        throw new Exception($"Số lượng Serial đã chọn ({det.SerialNumbers.Count}) không khớp với số lượng mua ({det.Quantity}) cho sản phẩm {det.ProductId}.");

                    // Check Product stock
                    var product = _unitOfWork.Products.GetById(det.ProductId);
                    if (product == null)
                        throw new Exception($"Không tìm thấy sản phẩm có ID {det.ProductId}.");
                    
                    if (product.QuantityInStock < det.Quantity)
                        throw new Exception($"Sản phẩm '{product.ProductName}' không đủ hàng tồn kho. Hiện còn: {product.QuantityInStock}");

                    // 2. Create OrderDetail
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = det.ProductId,
                        Quantity = det.Quantity,
                        UnitPrice = det.UnitPrice
                    };
                    _unitOfWork.OrderDetails.Add(orderDetail);
                    _unitOfWork.SaveChanges(); // Generate OrderDetailId

                    // 3. Update chosen ProductItems status to 'Sold' and bind to OrderDetail
                    foreach (var serial in det.SerialNumbers)
                    {
                        var item = _unitOfWork.ProductItems.GetById(serial);
                        if (item == null)
                            throw new Exception($"Không tìm thấy thiết bị mang số Serial '{serial}' trong hệ thống.");
                        if (item.Status != "InStock")
                            throw new Exception($"Thiết bị mang số Serial '{serial}' đã bị bán hoặc có trạng thái không khả dụng (Trạng thái hiện tại: {item.Status}).");

                        item.Status = "Sold";
                        item.OrderDetailId = orderDetail.OrderDetailId;
                        _unitOfWork.ProductItems.Update(item);
                    }

                    // 4. Update Product Stock
                    product.QuantityInStock -= det.Quantity;
                    _unitOfWork.Products.Update(product);

                    // 5. Add InventoryLog
                    var log = new InventoryLog
                    {
                        ProductId = det.ProductId,
                        ChangeType = "EXPORT",
                        QuantityChanged = -det.Quantity, // Negative for export
                        ReferenceId = order.OrderId,
                        Note = $"Xuất kho bán hàng. Mã hóa đơn: {order.OrderCode}",
                        CreatedAt = DateTime.Now
                    };
                    _unitOfWork.InventoryLogs.Add(log);
                }

                _unitOfWork.SaveChanges();
                _unitOfWork.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception("Lỗi khi tạo hóa đơn: " + ex.Message, ex);
            }
        }
    }
}
