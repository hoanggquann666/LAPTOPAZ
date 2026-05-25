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
    public class ProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        #region Categories
        public List<CategoryDTO> GetCategories()
        {
            return _unitOfWork.Categories.Query()
                .Select(c => new CategoryDTO
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ProductCount = c.Products.Count
                }).ToList();
        }

        public bool CreateCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (_unitOfWork.Categories.Any(c => c.CategoryName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return false;

            _unitOfWork.Categories.Add(new Category { CategoryName = name });
            return _unitOfWork.SaveChanges() > 0;
        }

        public bool UpdateCategory(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var cat = _unitOfWork.Categories.GetById(id);
            if (cat == null) return false;

            if (_unitOfWork.Categories.Any(c => c.CategoryName.Equals(name, StringComparison.OrdinalIgnoreCase) && c.CategoryId != id))
                return false;

            cat.CategoryName = name;
            _unitOfWork.Categories.Update(cat);
            return _unitOfWork.SaveChanges() > 0;
        }

        public bool DeleteCategory(int id)
        {
            var cat = _unitOfWork.Categories.GetById(id);
            if (cat == null || cat.Products.Any()) return false; // Prevent delete if has products
            
            _unitOfWork.Categories.Remove(cat);
            return _unitOfWork.SaveChanges() > 0;
        }
        #endregion

        #region Brands
        public List<BrandDTO> GetBrands()
        {
            return _unitOfWork.Brands.Query()
                .Select(b => new BrandDTO
                {
                    BrandId = b.BrandId,
                    BrandName = b.BrandName,
                    ProductCount = b.Products.Count
                }).ToList();
        }

        public bool CreateBrand(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (_unitOfWork.Brands.Any(b => b.BrandName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return false;

            _unitOfWork.Brands.Add(new Brand { BrandName = name });
            return _unitOfWork.SaveChanges() > 0;
        }

        public bool UpdateBrand(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var brand = _unitOfWork.Brands.GetById(id);
            if (brand == null) return false;

            if (_unitOfWork.Brands.Any(b => b.BrandName.Equals(name, StringComparison.OrdinalIgnoreCase) && b.BrandId != id))
                return false;

            brand.BrandName = name;
            _unitOfWork.Brands.Update(brand);
            return _unitOfWork.SaveChanges() > 0;
        }

        public bool DeleteBrand(int id)
        {
            var brand = _unitOfWork.Brands.GetById(id);
            if (brand == null || brand.Products.Any()) return false; // Prevent delete if has products
            
            _unitOfWork.Brands.Remove(brand);
            return _unitOfWork.SaveChanges() > 0;
        }
        #endregion

        #region Products
        public List<ProductDTO> GetProducts(string search = null, int? categoryId = null, int? brandId = null, bool? activeOnly = true)
        {
            var query = _unitOfWork.Products.Query().Include(p => p.Category).Include(p => p.Brand);

            if (activeOnly == true)
            {
                query = query.Where(p => p.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(search) || 
                                         p.ProductCode.ToLower().Contains(search) ||
                                         p.CPU.ToLower().Contains(search));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (brandId.HasValue && brandId > 0)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            return query.Select(p => new ProductDTO
            {
                ProductId = p.ProductId,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.CategoryName,
                BrandId = p.BrandId,
                BrandName = p.Brand.BrandName,
                CPU = p.CPU,
                RAM = p.RAM,
                GPU = p.GPU,
                Storage = p.Storage,
                ScreenSize = p.ScreenSize,
                ImportPrice = p.ImportPrice,
                SalePrice = p.SalePrice,
                QuantityInStock = p.QuantityInStock,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive
            }).ToList();
        }

        public ProductDTO GetProductById(int id)
        {
            var p = _unitOfWork.Products.Query()
                .Include(prod => prod.Category)
                .Include(prod => prod.Brand)
                .FirstOrDefault(prod => prod.ProductId == id);

            if (p == null) return null;

            return new ProductDTO
            {
                ProductId = p.ProductId,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.CategoryName,
                BrandId = p.BrandId,
                BrandName = p.Brand.BrandName,
                CPU = p.CPU,
                RAM = p.RAM,
                GPU = p.GPU,
                Storage = p.Storage,
                ScreenSize = p.ScreenSize,
                ImportPrice = p.ImportPrice,
                SalePrice = p.SalePrice,
                QuantityInStock = p.QuantityInStock,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive
            };
        }

        public bool CreateProduct(ProductDTO dto)
        {
            RolePermissions.EnsureCanManageProducts();
            if (dto == null) return false;
            // Validations
            if (string.IsNullOrWhiteSpace(dto.ProductCode) || string.IsNullOrWhiteSpace(dto.ProductName)) return false;
            if (dto.ImportPrice <= 0 || dto.SalePrice <= 0 || dto.SalePrice < dto.ImportPrice) return false;

            // Check unique code
            if (_unitOfWork.Products.Any(p => p.ProductCode.Equals(dto.ProductCode, StringComparison.OrdinalIgnoreCase)))
                return false;

            var product = new Product
            {
                ProductCode = dto.ProductCode,
                ProductName = dto.ProductName,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                CPU = dto.CPU ?? "",
                RAM = dto.RAM ?? "",
                GPU = dto.GPU ?? "",
                Storage = dto.Storage ?? "",
                ScreenSize = dto.ScreenSize ?? "",
                ImportPrice = dto.ImportPrice,
                SalePrice = dto.SalePrice,
                QuantityInStock = 0, // Starts at 0, updated by Import
                ImageUrl = dto.ImageUrl,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _unitOfWork.Products.Add(product);
            return _unitOfWork.SaveChanges() > 0;
        }

        public bool UpdateProduct(ProductDTO dto)
        {
            RolePermissions.EnsureCanManageProducts();
            if (dto == null) return false;
            if (string.IsNullOrWhiteSpace(dto.ProductName)) return false;
            if (dto.ImportPrice <= 0 || dto.SalePrice <= 0) return false;

            var product = _unitOfWork.Products.GetById(dto.ProductId);
            if (product == null) return false;

            // Update configuration
            product.ProductName = dto.ProductName;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;
            product.CPU = dto.CPU ?? "";
            product.RAM = dto.RAM ?? "";
            product.GPU = dto.GPU ?? "";
            product.Storage = dto.Storage ?? "";
            product.ScreenSize = dto.ScreenSize ?? "";
            product.ImportPrice = dto.ImportPrice;
            product.SalePrice = dto.SalePrice;
            product.ImageUrl = dto.ImageUrl;
            product.IsActive = dto.IsActive;

            _unitOfWork.Products.Update(product);
            return _unitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Xóa sản phẩm: soft-delete (IsActive=false) nếu còn ràng buộc; không xóa khi còn serial Sold/Reserved.
        /// </summary>
        public string DeleteProduct(int productId)
        {
            RolePermissions.EnsureCanManageProducts();

            var product = _unitOfWork.Products.GetById(productId);
            if (product == null)
                return "NOT_FOUND";

            if (_unitOfWork.ProductItems.Any(pi =>
                pi.ProductId == productId && (pi.Status == "Sold" || pi.Status == "Reserved")))
                return "HAS_ACTIVE_ITEMS";

            _unitOfWork.BeginTransaction();
            try
            {
                bool hasHistory =
                    _unitOfWork.OrderDetails.Any(od => od.ProductId == productId) ||
                    _unitOfWork.ImportReceiptDetails.Any(ird => ird.ProductId == productId);

                if (hasHistory)
                {
                    product.IsActive = false;
                    _unitOfWork.Products.Update(product);
                    _unitOfWork.SaveChanges();
                    _unitOfWork.CommitTransaction();
                    return "SOFT_DELETED";
                }

                var items = _unitOfWork.ProductItems.Find(pi => pi.ProductId == productId).ToList();
                foreach (var item in items)
                {
                    if (item.Status == "Sold" || item.Status == "Reserved")
                    {
                        _unitOfWork.RollbackTransaction();
                        return "HAS_ACTIVE_ITEMS";
                    }
                    _unitOfWork.ProductItems.Remove(item);
                }

                _unitOfWork.Products.Remove(product);
                _unitOfWork.SaveChanges();
                _unitOfWork.CommitTransaction();
                return "DELETED";
            }
            catch
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }
        #endregion

        #region Suppliers
        public List<SupplierDTO> GetSuppliers(string search = null)
        {
            var query = _unitOfWork.Suppliers.Query();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(s => s.SupplierName.ToLower().Contains(search) || s.Phone.Contains(search));
            }

            return query.Select(s => new SupplierDTO
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address
            }).ToList();
        }

        public bool CreateSupplier(SupplierDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.SupplierName) || string.IsNullOrWhiteSpace(dto.Phone))
                return false;

            _unitOfWork.Suppliers.Add(new Supplier
            {
                SupplierName = dto.SupplierName,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                CreatedAt = DateTime.Now
            });
            return _unitOfWork.SaveChanges() > 0;
        }

        public bool UpdateSupplier(SupplierDTO dto)
        {
            if (dto == null) return false;
            var s = _unitOfWork.Suppliers.GetById(dto.SupplierId);
            if (s == null) return false;

            s.SupplierName = dto.SupplierName;
            s.Phone = dto.Phone;
            s.Email = dto.Email;
            s.Address = dto.Address;

            _unitOfWork.Suppliers.Update(s);
            return _unitOfWork.SaveChanges() > 0;
        }
        #endregion

        #region Customers
        public List<CustomerDTO> GetCustomers(string search = null)
        {
            var query = _unitOfWork.Customers.Query();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(c => c.CustomerName.ToLower().Contains(search) || c.Phone.Contains(search));
            }

            return query.Select(c => new CustomerDTO
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public CustomerDTO GetCustomerByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            var c = _unitOfWork.Customers.Query().FirstOrDefault(cust => cust.Phone == phone);
            if (c == null) return null;

            return new CustomerDTO
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                CreatedAt = c.CreatedAt
            };
        }

        public bool CreateCustomer(CustomerDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CustomerName) || string.IsNullOrWhiteSpace(dto.Phone))
                return false;

            // Check unique phone
            if (_unitOfWork.Customers.Any(c => c.Phone == dto.Phone))
                return false;

            _unitOfWork.Customers.Add(new Customer
            {
                CustomerName = dto.CustomerName,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                CreatedAt = DateTime.Now
            });
            return _unitOfWork.SaveChanges() > 0;
        }

        public bool UpdateCustomer(CustomerDTO dto)
        {
            if (dto == null) return false;
            var c = _unitOfWork.Customers.GetById(dto.CustomerId);
            if (c == null) return false;

            c.CustomerName = dto.CustomerName;
            c.Phone = dto.Phone;
            c.Email = dto.Email;
            c.Address = dto.Address;

            _unitOfWork.Customers.Update(c);
            return _unitOfWork.SaveChanges() > 0;
        }
        #endregion
    }
}
