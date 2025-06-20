using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models.EquipmentManagement
{
    public class EquipmentLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BatchId { get; set; }
        [ForeignKey("BatchId")]
        public virtual EquipmentBatch Batch { get; set; } = null!;

        [Required]
        public int EquipmentTypeId { get; set; }
        [ForeignKey("EquipmentTypeId")]
        public virtual Equipment EquipmentType { get; set; } = null!;

        [Required]
        public int PlannedQty { get; set; }

        public int ReceivedQty { get; set; }

        [Required]
        public EquipmentStatus Status { get; set; } = EquipmentStatus.ClientWarehouse;

        public DateTime? ReceivedDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? InstalledDate { get; set; }

        // Navigation properties
        public virtual ICollection<EquipmentDiscrepancy> Discrepancies { get; set; } = new List<EquipmentDiscrepancy>();
        public virtual ICollection<JobTaskEquipmentLine> JobTaskEquipmentLines { get; set; } = new List<JobTaskEquipmentLine>();
    }
} 