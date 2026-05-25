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
        /// <summary>
        /// Đơn đặt trước (mã DH-): có bước giao hàng. Mua tại quầy (mã HD-) bỏ qua Shipping/Delivered.
        /// </summary>
        private static bool IsPlaceOrderCode(string orderCode) =>
            !string.IsNullOrEmpty(orderCode) && orderCode.StartsWith("DH-", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Đặt hàng: Pending + giữ serial ở trạng thái Reserved (chưa trừ tồn kho).
        /// Bắt buộc đã chọn đủ serial và còn đủ hàng trong kho.
        /// </summary>
        public int CreatePendingOrder(int customerId, int createdBy, decimal discount, List<OrderDetailDTO> details)
        {
            if (details == null || !details.Any())
                throw new ArgumentException("Giỏ hàng rỗng, không thể tạo đơn đặt hàng.");

            _unitOfWork.BeginTransaction();
            try
            {
                string orderCode = "DH-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);
                while (_unitOfWork.Orders.Any(o => o.OrderCode == orderCode))
                    orderCode = "DH-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);

                decimal totalAmount = details.Sum(d => d.Quantity * d.UnitPrice);
                decimal finalAmount = totalAmount - discount;
                if (finalAmount < 0) finalAmount = 0;

                var order = new Order
                {
                    OrderCode = orderCode,
                    CustomerId = customerId,
                    CreatedBy = createdBy,
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    DiscountAmount = discount,
                    FinalAmount = finalAmount,
                    Status = "Pending"
                };
                _unitOfWork.Orders.Add(order);
                _unitOfWork.SaveChanges();

                foreach (var det in details)
                {
                    if (det.Quantity <= 0)
                        throw new Exception("Số lượng đặt hàng phải lớn hơn 0.");
                    if (det.SerialNumbers == null || det.SerialNumbers.Count != det.Quantity)
                        throw new Exception($"Phải chọn đủ {det.Quantity} serial cho sản phẩm '{det.ProductName}' trước khi đặt hàng.");

                    var product = _unitOfWork.Products.GetById(det.ProductId);
                    if (product == null)
                        throw new Exception($"Không tìm thấy sản phẩm ID {det.ProductId}.");
                    if (product.QuantityInStock < det.Quantity)
                        throw new Exception($"'{product.ProductName}' không đủ tồn kho (còn {product.QuantityInStock}).");

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = det.ProductId,
                        Quantity = det.Quantity,
                        UnitPrice = det.UnitPrice
                    };
                    _unitOfWork.OrderDetails.Add(orderDetail);
                    _unitOfWork.SaveChanges();

                    foreach (var serial in det.SerialNumbers)
                    {
                        var item = _unitOfWork.ProductItems.GetById(serial);
                        if (item == null)
                            throw new Exception($"Không tìm thấy serial '{serial}'.");
                        if (item.Status != "InStock")
                            throw new Exception($"Serial '{serial}' không khả dụng (trạng thái: {item.Status}).");

                        item.Status = "Reserved";
                        item.OrderDetailId = orderDetail.OrderDetailId;
                        _unitOfWork.ProductItems.Update(item);
                    }
                }

                _unitOfWork.SaveChanges();
                _unitOfWork.CommitTransaction();
                return order.OrderId;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception("Lỗi khi tạo đơn đặt hàng: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng.
        /// Đặt hàng (DH-): Pending → Confirmed → Paid → Shipping → Delivered → Completed.
        /// Mua tại quầy (HD-): Paid → Completed.
        /// </summary>
        public bool UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = _unitOfWork.Orders.GetById(orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng.");

            string current = order.Status;
            bool isPlace = IsPlaceOrderCode(order.OrderCode);
            bool valid = false;
            switch (newStatus)
            {
                case "Confirmed":
                    valid = current == "Pending";
                    break;
                case "Paid":
                    valid = current == "Confirmed";
                    break;
                case "Shipping":
                    valid = current == "Paid" && isPlace;
                    break;
                case "Delivered":
                    valid = current == "Shipping" && isPlace;
                    break;
                case "Completed":
                    if (isPlace)
                        valid = current == "Delivered";
                    else
                        valid = current == "Paid";
                    break;
                case "Cancelled":
                    valid = current == "Pending" || current == "Confirmed";
                    break;
            }

            if (!valid)
                throw new Exception($"Không thể chuyển trạng thái từ '{current}' sang '{newStatus}'.");

            order.Status = newStatus;
            _unitOfWork.Orders.Update(order);
            _unitOfWork.SaveChanges();
            return true;
        }

        /// <summary>
        /// Xác nhận đơn đặt hàng Confirmed → Paid: gán serial, trừ kho, ghi log.
        /// </summary>
        public bool ConfirmAndPayOrder(int orderId, List<OrderDetailDTO> detailsWithSerials)
        {
            var order = _unitOfWork.Orders.GetById(orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng.");
            if (order.Status != "Confirmed")
                throw new Exception($"Đơn hàng phải ở trạng thái 'Confirmed' để thanh toán (hiện tại: '{order.Status}').");

            _unitOfWork.BeginTransaction();
            try
            {
                var existingDetails = _unitOfWork.OrderDetails.Query()
                    .Where(od => od.OrderId == orderId).ToList();

                foreach (var dbDet in existingDetails)
                {
                    var reservedItems = _unitOfWork.ProductItems.Find(
                        pi => pi.OrderDetailId == dbDet.OrderDetailId && pi.Status == "Reserved").ToList();

                    if (reservedItems.Count == dbDet.Quantity)
                    {
                        // Serial đã chọn lúc đặt hàng — chuyển Reserved → Sold và trừ kho
                        foreach (var item in reservedItems)
                        {
                            item.Status = "Sold";
                            _unitOfWork.ProductItems.Update(item);
                        }
                    }
                    else
                    {
                        var matchDet = detailsWithSerials?.FirstOrDefault(d => d.ProductId == dbDet.ProductId);
                        if (matchDet == null || matchDet.SerialNumbers.Count != dbDet.Quantity)
                            throw new Exception($"Chưa chọn đủ serial cho sản phẩm ID {dbDet.ProductId}. Cần {dbDet.Quantity} serial.");

                        foreach (var serial in matchDet.SerialNumbers)
                        {
                            var item = _unitOfWork.ProductItems.GetById(serial);
                            if (item == null)
                                throw new Exception($"Không tìm thấy serial '{serial}'.");
                            if (item.Status != "InStock")
                                throw new Exception($"Serial '{serial}' không khả dụng (status: {item.Status}).");

                            item.Status = "Sold";
                            item.OrderDetailId = dbDet.OrderDetailId;
                            _unitOfWork.ProductItems.Update(item);
                        }
                    }

                    var product = _unitOfWork.Products.GetById(dbDet.ProductId);
                    if (product == null)
                        throw new Exception($"Không tìm thấy sản phẩm ID {dbDet.ProductId}.");
                    if (product.QuantityInStock < dbDet.Quantity)
                        throw new Exception($"'{product.ProductName}' không đủ hàng. Còn: {product.QuantityInStock}");

                    product.QuantityInStock -= dbDet.Quantity;
                    _unitOfWork.Products.Update(product);

                    var log = new InventoryLog
                    {
                        ProductId = dbDet.ProductId,
                        ChangeType = "EXPORT",
                        QuantityChanged = -dbDet.Quantity,
                        ReferenceId = order.OrderId,
                        Note = $"Xuất kho thanh toán đơn đặt hàng {order.OrderCode}",
                        CreatedAt = DateTime.Now
                    };
                    _unitOfWork.InventoryLogs.Add(log);
                }

                order.Status = "Paid";
                _unitOfWork.Orders.Update(order);
                _unitOfWork.SaveChanges();
                _unitOfWork.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception("Lỗi xác nhận thanh toán: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Hủy đơn: Cancelled. Chỉ hoàn kho + serial khi Status = Paid (tránh double restore).
        /// Dùng transaction khi cập nhật nhiều bảng.
        /// </summary>
        public bool CancelOrder(int orderId, string reason = null)
        {
            var order = _unitOfWork.Orders.GetById(orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng.");

            if (order.Status == "Cancelled")
                throw new Exception("Đơn hàng đã bị hủy trước đó.");
            if (order.Status == "Completed")
                throw new Exception("Không thể hủy đơn hàng đã hoàn thành.");

            _unitOfWork.BeginTransaction();
            try
            {
                var details = _unitOfWork.OrderDetails.Query()
                    .Where(od => od.OrderId == orderId).ToList();

                foreach (var det in details)
                {
                    var reservedItems = _unitOfWork.ProductItems.Find(
                        pi => pi.OrderDetailId == det.OrderDetailId && pi.Status == "Reserved").ToList();
                    foreach (var item in reservedItems)
                    {
                        item.Status = "InStock";
                        item.OrderDetailId = null;
                        _unitOfWork.ProductItems.Update(item);
                    }
                }

                bool needRestoreStock = (order.Status == "Paid");

                if (needRestoreStock)
                {
                    foreach (var det in details)
                    {
                        var soldItems = _unitOfWork.ProductItems.Find(
                            pi => pi.OrderDetailId == det.OrderDetailId && pi.Status == "Sold").ToList();

                        foreach (var item in soldItems)
                        {
                            item.Status = "InStock";
                            item.OrderDetailId = null;
                            _unitOfWork.ProductItems.Update(item);
                        }

                        var product = _unitOfWork.Products.GetById(det.ProductId);
                        if (product != null)
                        {
                            product.QuantityInStock += det.Quantity;
                            _unitOfWork.Products.Update(product);
                        }

                        var log = new InventoryLog
                        {
                            ProductId = det.ProductId,
                            ChangeType = "CANCEL_RESTORE",
                            QuantityChanged = det.Quantity,
                            ReferenceId = order.OrderId,
                            Note = $"Hoàn kho do hủy đơn {order.OrderCode}" + (reason != null ? $". Lý do: {reason}" : ""),
                            CreatedAt = DateTime.Now
                        };
                        _unitOfWork.InventoryLogs.Add(log);
                    }
                }

                order.Status = "Cancelled";
                _unitOfWork.Orders.Update(order);
                _unitOfWork.SaveChanges();
                _unitOfWork.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception("Lỗi hủy đơn hàng: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Xóa một dòng chi tiết khỏi đơn Pending/Confirmed; giải phóng serial Reserved.
        /// </summary>
        public bool RemoveOrderDetailLine(int orderId, int orderDetailId)
        {
            var order = _unitOfWork.Orders.GetById(orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng.");
            if (order.Status != "Pending" && order.Status != "Confirmed")
                throw new Exception("Chỉ được xóa dòng khi đơn ở trạng thái Pending hoặc Confirmed.");

            var detail = _unitOfWork.OrderDetails.GetById(orderDetailId);
            if (detail == null || detail.OrderId != orderId)
                throw new Exception("Không tìm thấy dòng chi tiết trong đơn hàng.");

            _unitOfWork.BeginTransaction();
            try
            {
                var reservedItems = _unitOfWork.ProductItems.Find(
                    pi => pi.OrderDetailId == orderDetailId && pi.Status == "Reserved").ToList();
                foreach (var item in reservedItems)
                {
                    item.Status = "InStock";
                    item.OrderDetailId = null;
                    _unitOfWork.ProductItems.Update(item);
                }

                _unitOfWork.OrderDetails.Remove(detail);

                var remaining = _unitOfWork.OrderDetails.Query().Where(od => od.OrderId == orderId).ToList();
                order.TotalAmount = remaining.Sum(d => d.Quantity * d.UnitPrice);
                order.FinalAmount = order.TotalAmount - order.DiscountAmount;
                if (order.FinalAmount < 0) order.FinalAmount = 0;
                _unitOfWork.Orders.Update(order);

                _unitOfWork.SaveChanges();
                _unitOfWork.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception("Lỗi xóa dòng đơn hàng: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Hóa đơn đã hoàn thành — dùng cho lịch sử in và đếm dashboard.
        /// </summary>
        public List<OrderDTO> GetCompletedOrders(string search = null)
        {
            var query = _unitOfWork.Orders.Query()
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Where(o => o.Status == "Completed");

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

        /// <summary>
        /// Lấy danh sách đơn hàng lọc theo trạng thái.
        /// </summary>
        public List<OrderDTO> GetOrdersByStatus(string status)
        {
            var query = _unitOfWork.Orders.Query()
                .Include(o => o.Customer)
                .Include(o => o.User);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.Status == status);

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
    }
}
