using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopAZ.Models
{
    [Table("ImportReceipts")]
    public class ImportReceipt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImportReceiptId { get; set; }

        public int SupplierId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime ImportDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; } = 0;

        // Navigation properties
        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User User { get; set; }

        public virtual ICollection<ImportReceiptDetail> ImportReceiptDetails { get; set; } = new HashSet<ImportReceiptDetail>();
    }
}
