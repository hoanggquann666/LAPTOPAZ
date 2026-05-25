using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LaptopAZ.DTO;
using LaptopAZ.Helpers;
using LaptopAZ.Models;
using LaptopAZ.Repository;

namespace LaptopAZ.BLL
{
    public class ReturnService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReturnService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public List<ReturnDTO> GetAllReturns()
        {
            return _unitOfWork.Returns.Query()
                .Include(r => r.Order)
                .Include(r => r.Order.Customer)
                .Include(r => r.User)
                .Include(r => r.ReturnDetails)
                .OrderByDescending(r => r.ReturnDate)
                .Select(r => new ReturnDTO
                {
                    ReturnId = r.ReturnId,
                    OrderId = r.OrderId,
                    OrderCode = r.Order.OrderCode,
                    CustomerName = r.Order.Customer.CustomerName,
                    CreatedBy = r.CreatedBy,
                    EmployeeName = r.User.FullName,
                    ReturnDate = r.ReturnDate,
                    Reason = r.Reason,
                    ReturnedSerials = r.ReturnDetails.Select(rd => rd.SerialNumber).ToList()
                }).ToList();
        }

        /// <summary>
        /// Processes a customer return under a transaction, updating specific serial status to Defective, restoring stock and logging changes.
        /// </summary>
        public bool CreateReturn(int orderId, int createdBy, string reason, List<string> serialNumbers)
        {
            RolePermissions.EnsureCanMutateBusinessData();
            if (serialNumbers == null || !serialNumbers.Any())
                throw new ArgumentException("Danh sách số Serial trả hàng rỗng.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Lý do trả hàng không được để trống.");

            var order = _unitOfWork.Orders.GetById(orderId);
            if (order == null)
                throw new Exception($"Không tìm thấy hóa đơn có ID {orderId}.");

            _unitOfWork.BeginTransaction();
            try
            {
                // 1. Create Return
                var ret = new Return
                {
                    OrderId = orderId,
                    CreatedBy = createdBy,
                    ReturnDate = DateTime.Now,
                    Reason = reason
                };
                _unitOfWork.Returns.Add(ret);
                _unitOfWork.SaveChanges(); // Generate ReturnId

                // Group by Product ID to update quantities efficiently
                var productQuantities = new Dictionary<int, int>();

                // 2. Process each serial number
                foreach (var serial in serialNumbers)
                {
                    var item = _unitOfWork.ProductItems.GetById(serial);
                    if (item == null)
                        throw new Exception($"Không tìm thấy thiết bị mang số Serial '{serial}' trong hệ thống.");
                    
                    if (item.Status != "Sold")
                        throw new Exception($"Thiết bị mang số Serial '{serial}' không ở trạng thái 'Sold' (Trạng thái hiện tại: {item.Status}).");

                    // Verify it belongs to this order
                    if (item.OrderDetail == null || item.OrderDetail.OrderId != orderId)
                        throw new Exception($"Thiết bị mang số Serial '{serial}' không thuộc hóa đơn này.");

                    // Create ReturnDetail
                    var detail = new ReturnDetail
                    {
                        ReturnId = ret.ReturnId,
                        SerialNumber = serial,
                        Quantity = 1
                    };
                    _unitOfWork.ReturnDetails.Add(detail);

                    // Update ProductItem status to Defective (Returned laptop is treated as defective for inspection)
                    item.Status = "Defective";
                    _unitOfWork.ProductItems.Update(item);

                    // Track count to restore Product stock
                    int prodId = item.ProductId;
                    if (productQuantities.ContainsKey(prodId))
                        productQuantities[prodId]++;
                    else
                        productQuantities[prodId] = 1;
                }

                // 3. Update stock and log inventory for each affected product
                foreach (var kvp in productQuantities)
                {
                    int prodId = kvp.Key;
                    int qty = kvp.Value;

                    var product = _unitOfWork.Products.GetById(prodId);
                    if (product == null)
                        throw new Exception($"Không tìm thấy sản phẩm có ID {prodId}.");

                    // Increase stock by returned quantity
                    product.QuantityInStock += qty;
                    _unitOfWork.Products.Update(product);

                    // Add InventoryLog
                    var log = new InventoryLog
                    {
                        ProductId = prodId,
                        ChangeType = "RETURN_CUSTOMER",
                        QuantityChanged = qty,
                        ReferenceId = ret.ReturnId,
                        Note = $"Nhận trả hàng {qty} máy từ khách hàng. Mã phiếu trả: #{ret.ReturnId}",
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
                throw new Exception("Lỗi khi hoàn trả hàng: " + ex.Message, ex);
            }
        }
    }
}
