using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopAZ.Models
{
    [Table("ReturnDetails")]
    public class ReturnDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReturnDetailId { get; set; }

        public int ReturnId { get; set; }

        [Required]
        [StringLength(50)]
        public string SerialNumber { get; set; }

        public int Quantity { get; set; } = 1;

        // Navigation properties
        [ForeignKey("ReturnId")]
        public virtual Return Return { get; set; }

        [ForeignKey("SerialNumber")]
        public virtual ProductItem ProductItem { get; set; }
    }
}
