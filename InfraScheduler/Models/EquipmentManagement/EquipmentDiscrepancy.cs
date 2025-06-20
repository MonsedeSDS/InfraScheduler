using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models.EquipmentManagement
{
    public class EquipmentDiscrepancy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LineId { get; set; }
        [ForeignKey("LineId")]
        public virtual EquipmentLine Line { get; set; } = null!;

        [Required]
        public int PlannedQty { get; set; }

        [Required]
        public int ReceivedQty { get; set; }

        [StringLength(500)]
        public string Note { get; set; } = string.Empty;
    }
} 