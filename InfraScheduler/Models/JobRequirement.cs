using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class JobRequirement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JobId { get; set; }
        [ForeignKey("JobId")]
        public virtual Job Job { get; set; } = null!;

        [Required]
        public int EquipmentTypeId { get; set; }
        [ForeignKey("EquipmentTypeId")]
        public virtual EquipmentManagement.Equipment EquipmentType { get; set; } = null!;

        [Required]
        public int PlannedQty { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string Priority { get; set; } = "Normal"; // High, Normal, Low

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;
    }
} 