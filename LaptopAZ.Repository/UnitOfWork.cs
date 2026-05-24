using System;
using System.Data.Entity;
using LaptopAZ.DAL;
using LaptopAZ.Models;

namespace LaptopAZ.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LaptopAZDbContext _context;
        private DbContextTransaction _transaction;

        public UnitOfWork()
        {
            _context = new LaptopAZDbContext();
        }

        private IGenericRepository<Role> _roles;
        private IGenericRepository<User> _users;
        private IGenericRepository<Category> _categories;
        private IGenericRepository<Brand> _brands;
        private IGenericRepository<Product> _products;
        private IGenericRepository<Supplier> _suppliers;
        private IGenericRepository<Customer> _customers;
        private IGenericRepository<ImportReceipt> _importReceipts;
        private IGenericRepository<ImportReceiptDetail> _importReceiptDetails;
        private IGenericRepository<Order> _orders;
        private IGenericRepository<OrderDetail> _orderDetails;
        private IGenericRepository<ProductItem> _productItems;
        private IGenericRepository<InventoryLog> _inventoryLogs;
        private IGenericRepository<Return> _returns;
        private IGenericRepository<ReturnDetail> _returnDetails;

        public IGenericRepository<Role> Roles => _roles ?? (_roles = new GenericRepository<Role>(_context));
        public IGenericRepository<User> Users => _users ?? (_users = new GenericRepository<User>(_context));
        public IGenericRepository<Category> Categories => _categories ?? (_categories = new GenericRepository<Category>(_context));
        public IGenericRepository<Brand> Brands => _brands ?? (_brands = new GenericRepository<Brand>(_context));
        public IGenericRepository<Product> Products => _products ?? (_products = new GenericRepository<Product>(_context));
        public IGenericRepository<Supplier> Suppliers => _suppliers ?? (_suppliers = new GenericRepository<Supplier>(_context));
        public IGenericRepository<Customer> Customers => _customers ?? (_customers = new GenericRepository<Customer>(_context));
        public IGenericRepository<ImportReceipt> ImportReceipts => _importReceipts ?? (_importReceipts = new GenericRepository<ImportReceipt>(_context));
        public IGenericRepository<ImportReceiptDetail> ImportReceiptDetails => _importReceiptDetails ?? (_importReceiptDetails = new GenericRepository<ImportReceiptDetail>(_context));
        public IGenericRepository<Order> Orders => _orders ?? (_orders = new GenericRepository<Order>(_context));
        public IGenericRepository<OrderDetail> OrderDetails => _orderDetails ?? (_orderDetails = new GenericRepository<OrderDetail>(_context));
        public IGenericRepository<ProductItem> ProductItems => _productItems ?? (_productItems = new GenericRepository<ProductItem>(_context));
        public IGenericRepository<InventoryLog> InventoryLogs => _inventoryLogs ?? (_inventoryLogs = new GenericRepository<InventoryLog>(_context));
        public IGenericRepository<Return> Returns => _returns ?? (_returns = new GenericRepository<Return>(_context));
        public IGenericRepository<ReturnDetail> ReturnDetails => _returnDetails ?? (_returnDetails = new GenericRepository<ReturnDetail>(_context));

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public void BeginTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
