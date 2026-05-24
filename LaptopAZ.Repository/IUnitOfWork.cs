using System;
using LaptopAZ.Models;

namespace LaptopAZ.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Role> Roles { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Brand> Brands { get; }
        IGenericRepository<Product> Products { get; }
        IGenericRepository<Supplier> Suppliers { get; }
        IGenericRepository<Customer> Customers { get; }
        IGenericRepository<ImportReceipt> ImportReceipts { get; }
        IGenericRepository<ImportReceiptDetail> ImportReceiptDetails { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<OrderDetail> OrderDetails { get; }
        IGenericRepository<ProductItem> ProductItems { get; }
        IGenericRepository<InventoryLog> InventoryLogs { get; }
        IGenericRepository<Return> Returns { get; }
        IGenericRepository<ReturnDetail> ReturnDetails { get; }

        int SaveChanges();
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}
