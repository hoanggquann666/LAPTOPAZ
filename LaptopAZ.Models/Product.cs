using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopAZ.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; }

        [Required]
        [StringLength(255)]
        public string ProductName { get; set; }

        public int CategoryId { get; set; }

        public int BrandId { get; set; }

        [Required]
        [StringLength(255)]
        public string CPU { get; set; }

        [Required]
        [StringLength(255)]
        public string RAM { get; set; }

        [StringLength(255)]
        public string GPU { get; set; }

        [Required]
        [StringLength(255)]
        public string Storage { get; set; }

        [StringLength(255)]
        public string ScreenSize { get; set; }

        public decimal ImportPrice { get; set; }

        public decimal SalePrice { get; set; }

        public int QuantityInStock { get; set; } = 0;

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }

        public virtual ICollection<ImportReceiptDetail> ImportReceiptDetails { get; set; } = new HashSet<ImportReceiptDetail>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
        public virtual ICollection<ProductItem> ProductItems { get; set; } = new HashSet<ProductItem>();
        public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new HashSet<InventoryLog>();
    }
}
