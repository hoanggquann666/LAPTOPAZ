using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopAZ.Models
{
    [Table("ProductItems")]
    public class ProductItem
    {
        [Key]
        [StringLength(50)]
        [Required]
        public string SerialNumber { get; set; }

        public int ProductId { get; set; }

        public int ImportDetailId { get; set; }

        public int? OrderDetailId { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "InStock"; // 'InStock', 'Sold', 'Warranty', 'Defective'

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("ImportDetailId")]
        public virtual ImportReceiptDetail ImportReceiptDetail { get; set; }

        [ForeignKey("OrderDetailId")]
        public virtual OrderDetail OrderDetail { get; set; }

        public virtual ICollection<ReturnDetail> ReturnDetails { get; set; } = new HashSet<ReturnDetail>();
    }
}
