using System;
using System.Data.Entity;
using LaptopAZ.Models;

namespace LaptopAZ.DAL
{
    public class LaptopAZDbContext : DbContext
    {
        public LaptopAZDbContext() : base("name=LaptopAZDbContext")
        {
            // Configure EF behavior
            Configuration.LazyLoadingEnabled = true;
            Configuration.ProxyCreationEnabled = true;
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ImportReceipt> ImportReceipts { get; set; }
        public DbSet<ImportReceiptDetail> ImportReceiptDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ProductItem> ProductItems { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<ReturnDetail> ReturnDetails { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure cascade deletes or key mappings if necessary
            
            // ProductItem -> Product (1-n)
            modelBuilder.Entity<ProductItem>()
                .HasRequired(pi => pi.Product)
                .WithMany(p => p.ProductItems)
                .HasForeignKey(pi => pi.ProductId)
                .WillCascadeOnDelete(false);

            // ProductItem -> ImportReceiptDetail (1-n)
            modelBuilder.Entity<ProductItem>()
                .HasRequired(pi => pi.ImportReceiptDetail)
                .WithMany(ird => ird.ProductItems)
                .HasForeignKey(pi => pi.ImportDetailId)
                .WillCascadeOnDelete(false);

            // ProductItem -> OrderDetail (0..1 - n)
            modelBuilder.Entity<ProductItem>()
                .HasOptional(pi => pi.OrderDetail)
                .WithMany(od => od.ProductItems)
                .HasForeignKey(pi => pi.OrderDetailId)
                .WillCascadeOnDelete(false);

            // ReturnDetail -> ProductItem (1-n)
            modelBuilder.Entity<ReturnDetail>()
                .HasRequired(rd => rd.ProductItem)
                .WithMany(pi => pi.ReturnDetails)
                .HasForeignKey(rd => rd.SerialNumber)
                .WillCascadeOnDelete(false);

            // Order -> Customer (1-n)
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .WillCascadeOnDelete(false);

            // Order -> User (1-n)
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CreatedBy)
                .WillCascadeOnDelete(false);

            // ImportReceipt -> Supplier (1-n)
            modelBuilder.Entity<ImportReceipt>()
                .HasRequired(ir => ir.Supplier)
                .WithMany(s => s.ImportReceipts)
                .HasForeignKey(ir => ir.SupplierId)
                .WillCascadeOnDelete(false);

            // ImportReceipt -> User (1-n)
            modelBuilder.Entity<ImportReceipt>()
                .HasRequired(ir => ir.User)
                .WithMany(u => u.ImportReceipts)
                .HasForeignKey(ir => ir.CreatedBy)
                .WillCascadeOnDelete(false);

            // Return -> Order (1-n)
            modelBuilder.Entity<Return>()
                .HasRequired(r => r.Order)
                .WithMany(o => o.Returns)
                .HasForeignKey(r => r.OrderId)
                .WillCascadeOnDelete(false);

            // Return -> User (1-n)
            modelBuilder.Entity<Return>()
                .HasRequired(r => r.User)
                .WithMany(u => u.Returns)
                .HasForeignKey(r => r.CreatedBy)
                .WillCascadeOnDelete(false);
        }
    }
}
