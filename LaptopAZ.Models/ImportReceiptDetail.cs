using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopAZ.Models
{
    [Table("ImportReceiptDetails")]
    public class ImportReceiptDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImportDetailId { get; set; }

        public int ImportReceiptId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal ImportPrice { get; set; }

        // Navigation properties
        [ForeignKey("ImportReceiptId")]
        public virtual ImportReceipt ImportReceipt { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public virtual ICollection<ProductItem> ProductItems { get; set; } = new HashSet<ProductItem>();
    }
}
