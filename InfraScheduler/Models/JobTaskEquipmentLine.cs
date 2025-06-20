using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class JobTaskEquipmentLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JobTaskId { get; set; }
        [ForeignKey("JobTaskId")]
        public virtual JobTask JobTask { get; set; } = null!;

        [Required]
        public int EquipmentLineId { get; set; }
        [ForeignKey("EquipmentLineId")]
        public virtual EquipmentManagement.EquipmentLine EquipmentLine { get; set; } = null!;

        [Required]
        public int Quantity { get; set; } = 1;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
} 