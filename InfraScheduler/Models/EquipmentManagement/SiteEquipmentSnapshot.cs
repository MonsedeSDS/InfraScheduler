using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models.EquipmentManagement
{
    public class SiteEquipmentSnapshot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SiteId { get; set; }
        [ForeignKey("SiteId")]
        public virtual Site Site { get; set; } = null!;

        [Required]
        public int EquipmentTypeId { get; set; }
        [ForeignKey("EquipmentTypeId")]
        public virtual Equipment EquipmentType { get; set; } = null!;

        [Required]
        public int CurrentQty { get; set; }

        [Required]
        public DateTime LastUpdateUtc { get; set; }
    }
} 