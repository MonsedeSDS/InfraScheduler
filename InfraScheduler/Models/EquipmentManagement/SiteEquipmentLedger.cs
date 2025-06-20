using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models.EquipmentManagement
{
    public class SiteEquipmentLedger
    {
        [Key]
        public int LedgerId { get; set; }

        [Required]
        public int SiteId { get; set; }
        [ForeignKey("SiteId")]
        public virtual Site Site { get; set; } = null!;

        [Required]
        public int EquipmentTypeId { get; set; }
        [ForeignKey("EquipmentTypeId")]
        public virtual Equipment EquipmentType { get; set; } = null!;

        [Required]
        public int QuantityInstalled { get; set; } // Can be negative for removals

        [Required]
        public DateTime InstallationDate { get; set; }

        [Required]
        public int SourceJobId { get; set; }
        [ForeignKey("SourceJobId")]
        public virtual Job SourceJob { get; set; } = null!;

        [Required]
        public int BatchId { get; set; }
        [ForeignKey("BatchId")]
        public virtual EquipmentBatch Batch { get; set; } = null!;

        [Required]
        public int LineId { get; set; }
        [ForeignKey("LineId")]
        public virtual EquipmentLine Line { get; set; } = null!;

        [StringLength(1000)]
        public string SerialNumbers { get; set; } = string.Empty;
    }
} 