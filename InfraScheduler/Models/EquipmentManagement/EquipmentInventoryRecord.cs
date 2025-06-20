using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models.EquipmentManagement
{
    public class EquipmentInventoryRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EquipmentTypeId { get; set; }
        [ForeignKey("EquipmentTypeId")]
        public virtual Equipment EquipmentType { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public InventoryLocation Location { get; set; } = InventoryLocation.SDSWarehouse;

        [Required]
        public bool ReservedForSite { get; set; } = false;

        public int? SiteId { get; set; }
        [ForeignKey("SiteId")]
        public virtual Site? Site { get; set; }
    }
} 