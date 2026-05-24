using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopAZ.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderCode { get; set; }

        public int CustomerId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; } = 0;

        public decimal DiscountAmount { get; set; } = 0;

        public decimal FinalAmount { get; set; } = 0;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Paid"; // 'Pending', 'Paid', 'Cancelled'

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User User { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
        public virtual ICollection<Return> Returns { get; set; } = new HashSet<Return>();
    }
}
