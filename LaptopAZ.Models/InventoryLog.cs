using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopAZ.Models
{
    [Table("InventoryLogs")]
    public class InventoryLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventoryLogId { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(30)]
        public string ChangeType { get; set; } // 'IMPORT', 'EXPORT', 'RETURN_CUSTOMER', 'RETURN_SUPPLIER'

        public int QuantityChanged { get; set; }

        public int ReferenceId { get; set; } // Reference ID (ImportReceiptId or OrderId or ReturnId)

        [StringLength(255)]
        public string Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
