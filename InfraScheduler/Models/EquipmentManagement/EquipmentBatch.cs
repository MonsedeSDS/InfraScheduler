using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models.EquipmentManagement
{
    public class EquipmentBatch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JobId { get; set; }
        [ForeignKey("JobId")]
        public virtual Job Job { get; set; } = null!;

        [Required]
        public int SiteId { get; set; }
        [ForeignKey("SiteId")]
        public virtual Site Site { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<EquipmentLine> Lines { get; set; } = new List<EquipmentLine>();
    }
} 