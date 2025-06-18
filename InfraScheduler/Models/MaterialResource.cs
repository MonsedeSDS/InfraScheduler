using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class MaterialResource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime AvailableFrom { get; set; }

        [Required]
        public DateTime AvailableTo { get; set; }

        [Required]
        public string Status { get; set; } = "Available";

        [ForeignKey("MaterialId")]
        public virtual Material? Material { get; set; }

        // Navigation properties
        public virtual ICollection<MaterialReservation>? Reservations { get; set; }
    }
} 