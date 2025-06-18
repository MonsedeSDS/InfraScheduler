using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class MaterialReservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaterialResourceId { get; set; }

        [Required]
        public int JobTaskId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime ReservedFrom { get; set; }

        [Required]
        public DateTime ReservedTo { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [ForeignKey("MaterialResourceId")]
        public virtual MaterialResource? MaterialResource { get; set; }

        [ForeignKey("JobTaskId")]
        public virtual JobTask? JobTask { get; set; }
    }
}