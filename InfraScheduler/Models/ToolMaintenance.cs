using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class ToolMaintenance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ToolId { get; set; }

        [Required]
        public DateTime MaintenanceDate { get; set; }

        [Required]
        [StringLength(50)]
        public string MaintenanceType { get; set; } = string.Empty; // Regular, Repair, Calibration

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string PerformedBy { get; set; } = string.Empty;

        public decimal Cost { get; set; }

        [ForeignKey("ToolId")]
        public virtual Tool Tool { get; set; } = null!;
    }
} 