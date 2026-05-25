using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LaptopAZ.DTO;
using LaptopAZ.Models;
using LaptopAZ.Repository;

namespace LaptopAZ.BLL
{
    public class WarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WarehouseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public List<ImportReceiptDTO> GetAllImportReceipts(string search = null)
        {
            var query = _unitOfWork.ImportReceipts.Query()
                .Include(ir => ir.Supplier)
                .Include(ir => ir.User);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(ir => ir.Supplier.SupplierName.ToLower().Contains(search) ||
                                          ir.ImportReceiptId.ToString().Contains(search));
            }

            return query.OrderByDescending(ir => ir.ImportDate)
                .Select(ir => new ImportReceiptDTO
                {
                    ImportReceiptId = ir.ImportReceiptId,
                    SupplierId = ir.SupplierId,
                    SupplierName = ir.Supplier.SupplierName,
                    CreatedBy = ir.CreatedBy,
                    EmployeeName = ir.User.FullName,
                    ImportDate = ir.ImportDate,
                    TotalAmount = ir.TotalAmount
                }).ToList();
        }

        public ImportReceiptDTO GetImportReceiptById(int importReceiptId)
        {
            var ir = _unitOfWork.ImportReceipts.Query()
                .Include(x => x.Supplier)
                .Include(x => x.User)
                .FirstOrDefault(x => x.ImportReceiptId == importReceiptId);
            if (ir == null) return null;

            return new ImportReceiptDTO
            {
                ImportReceiptId = ir.ImportReceiptId,
                SupplierId = ir.SupplierId,
                SupplierName = ir.Supplier.SupplierName,
                CreatedBy = ir.CreatedBy,
                EmployeeName = ir.User.FullName,
                ImportDate = ir.ImportDate,
                TotalAmount = ir.TotalAmount
            };
        }

        public List<ImportReceiptDetailDTO> GetImportReceiptDetails(int importReceiptId)
        {
            return _unitOfWork.ImportReceiptDetails.Query()
                .Include(ird => ird.Product)
                .Include(ird => ird.ProductItems)
                .Where(ird => ird.ImportReceiptId == importReceiptId)
                .Select(ird => new ImportReceiptDetailDTO
                {
                    ImportDetailId = ird.ImportDetailId,
                    ImportReceiptId = ird.ImportReceiptId,
                    ProductId = ird.ProductId,
                    ProductName = ird.Product.ProductName,
                    Quantity = ird.Quantity,
                    ImportPrice = ird.ImportPrice,
                    SerialNumbers = ird.ProductItems.Select(pi => pi.SerialNumber).ToList()
                }).ToList();
        }

        public List<ProductItemDTO> GetProductItemsInStock(int productId)
        {
            return _unitOfWork.ProductItems.Query()
                .Where(pi => pi.ProductId == productId && pi.Status == "InStock")
                .Select(pi => new ProductItemDTO
                {
                    SerialNumber = pi.SerialNumber,
                    ProductId = pi.ProductId,
                    ProductName = pi.Product.ProductName,
                    ImportDetailId = pi.ImportDetailId,
                    OrderDetailId = pi.OrderDetailId,
                    Status = pi.Status
                }).ToList();
        }

        /// <summary>
        /// Imports a batch of laptops under a database transaction, adding serial numbers, updating stocks and logging inventory logs.
        /// </summary>
        public bool CreateImportReceipt(int supplierId, int createdBy, List<ImportReceiptDetailDTO> details)
        {
            if (details == null || !details.Any())
                return false;

            _unitOfWork.BeginTransaction();
            try
            {
                // 1. Create ImportReceipt
                var receipt = new ImportReceipt
                {
                    SupplierId = supplierId,
                    CreatedBy = createdBy,
                    ImportDate = DateTime.Now,
                    TotalAmount = details.Sum(d => d.Quantity * d.ImportPrice)
                };
                _unitOfWork.ImportReceipts.Add(receipt);
                _unitOfWork.SaveChanges(); // To get the generated ImportReceiptId

                foreach (var det in details)
                {
                    // Validation
                    if (det.Quantity <= 0 || det.ImportPrice <= 0)
                        throw new Exception("Số lượng và giá nhập phải lớn hơn 0.");
                    if (det.SerialNumbers.Count != det.Quantity)
                        throw new Exception($"Số lượng Serial ({det.SerialNumbers.Count}) không khớp với số lượng nhập ({det.Quantity}) cho sản phẩm ID {det.ProductId}.");

                    // Check if any serial number already exists in DB
                    foreach (var serial in det.SerialNumbers)
                    {
                        if (string.IsNullOrWhiteSpace(serial))
                            throw new Exception("Số Serial không được để trống.");
                        if (_unitOfWork.ProductItems.Any(pi => pi.SerialNumber == serial))
                            throw new Exception($"Số Serial '{serial}' đã tồn tại trong hệ thống.");
                    }

                    // 2. Create ImportReceiptDetail
                    var receiptDetail = new ImportReceiptDetail
                    {
                        ImportReceiptId = receipt.ImportReceiptId,
                        ProductId = det.ProductId,
                        Quantity = det.Quantity,
                        ImportPrice = det.ImportPrice
                    };
                    _unitOfWork.ImportReceiptDetails.Add(receiptDetail);
                    _unitOfWork.SaveChanges(); // To get ImportDetailId

                    // 3. Add ProductItems (Serial numbers)
                    foreach (var serial in det.SerialNumbers)
                    {
                        var item = new ProductItem
                        {
                            SerialNumber = serial,
                            ProductId = det.ProductId,
                            ImportDetailId = receiptDetail.ImportDetailId,
                            OrderDetailId = null,
                            Status = "InStock"
                        };
                        _unitOfWork.ProductItems.Add(item);
                    }

                    // 4. Update Product Stock and standard default Import Price reference
                    var product = _unitOfWork.Products.GetById(det.ProductId);
                    if (product == null)
                        throw new Exception($"Không tìm thấy sản phẩm có ID {det.ProductId}.");
                    
                    product.QuantityInStock += det.Quantity;
                    product.ImportPrice = det.ImportPrice; // Keep track of latest import price
                    _unitOfWork.Products.Update(product);

                    // 5. Add InventoryLog
                    var log = new InventoryLog
                    {
                        ProductId = det.ProductId,
                        ChangeType = "IMPORT",
                        QuantityChanged = det.Quantity,
                        ReferenceId = receipt.ImportReceiptId,
                        Note = $"Nhập kho {det.Quantity} máy. Mã phiếu nhập: #{receipt.ImportReceiptId}",
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
                throw new Exception("Lỗi khi nhập kho: " + ex.Message, ex);
            }
        }
        /// <summary>
        /// Gets all ProductItems (serials) for a given product, including those already sold or defective.
        /// </summary>
        public List<ProductItemDTO> GetAllProductItems(int productId)
        {
            return _unitOfWork.ProductItems.Query()
                .Where(pi => pi.ProductId == productId)
                .Select(pi => new ProductItemDTO
                {
                    SerialNumber = pi.SerialNumber,
                    ProductId = pi.ProductId,
                    ProductName = pi.Product.ProductName,
                    ImportDetailId = pi.ImportDetailId,
                    OrderDetailId = pi.OrderDetailId,
                    Status = pi.Status
                }).ToList();
        }

        /// <summary>
        /// Updates a serial number safely by creating a new record and deleting the old one.
        /// Only allows updating serials that are NOT 'Sold'.
        /// </summary>
        public bool UpdateSerialNumber(string oldSerial, string newSerial)
        {
            if (string.IsNullOrWhiteSpace(oldSerial) || string.IsNullOrWhiteSpace(newSerial))
                throw new ArgumentException("Số serial không được để trống.");

            newSerial = newSerial.Trim();

            if (oldSerial == newSerial) return true; // No change needed

            var existing = _unitOfWork.ProductItems.GetById(oldSerial);
            if (existing == null)
                throw new Exception($"Không tìm thấy serial '{oldSerial}' trong hệ thống.");

            if (existing.Status == "Sold")
                throw new Exception($"Không thể sửa serial '{oldSerial}' vì thiết bị này đã được bán.");

            if (_unitOfWork.ProductItems.Any(pi => pi.SerialNumber == newSerial))
                throw new Exception($"Serial '{newSerial}' đã tồn tại trong hệ thống. Vui lòng nhập serial khác.");

            _unitOfWork.BeginTransaction();
            try
            {
                var newItem = new LaptopAZ.Models.ProductItem
                {
                    SerialNumber = newSerial,
                    ProductId = existing.ProductId,
                    ImportDetailId = existing.ImportDetailId,
                    OrderDetailId = existing.OrderDetailId,
                    Status = existing.Status
                };

                _unitOfWork.ProductItems.Remove(existing);
                _unitOfWork.SaveChanges();
                _unitOfWork.ProductItems.Add(newItem);
                _unitOfWork.SaveChanges();
                _unitOfWork.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception("Lỗi khi cập nhật serial: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Deletes a serial number only if it is NOT 'Sold', and decrements QuantityInStock.
        /// </summary>
        public bool DeleteSerialNumber(string serial)
        {
            if (string.IsNullOrWhiteSpace(serial))
                throw new ArgumentException("Số serial không được để trống.");

            var item = _unitOfWork.ProductItems.GetById(serial);
            if (item == null)
                throw new Exception($"Không tìm thấy serial '{serial}' trong hệ thống.");

            if (item.Status == "Sold")
                throw new Exception($"Không thể xóa serial '{serial}' vì thiết bị này đã được bán cho khách hàng.");

            _unitOfWork.BeginTransaction();
            try
            {
                var product = _unitOfWork.Products.GetById(item.ProductId);
                if (product != null && product.QuantityInStock > 0)
                {
                    product.QuantityInStock -= 1;
                    _unitOfWork.Products.Update(product);
                }

                _unitOfWork.ProductItems.Remove(item);
                _unitOfWork.SaveChanges();
                _unitOfWork.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception("Lỗi khi xóa serial: " + ex.Message, ex);
            }
        }
    }
}
