using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class MaterialDelivery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }

        [Required]
        public double Quantity { get; set; }

        [Required]
        public DateTime DeliveryDate { get; set; }

        [Required]
        public string Supplier { get; set; }

        public string Notes { get; set; }

        [Required]
        public string Status { get; set; } // e.g., "Ordered", "In Transit", "Delivered"
    }
}