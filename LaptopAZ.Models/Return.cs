using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopAZ.Models
{
    [Table("Returns")]
    public class Return
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReturnId { get; set; }

        public int OrderId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime ReturnDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(255)]
        public string Reason { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User User { get; set; }

        public virtual ICollection<ReturnDetail> ReturnDetails { get; set; } = new HashSet<ReturnDetail>();
    }
}
