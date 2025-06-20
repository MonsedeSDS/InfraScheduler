using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models.EquipmentManagement
{
    public class Equipment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ModelNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Available"; // Available, In Use, Under Maintenance, Out of Service

        [Required]
        [StringLength(20)]
        public string Condition { get; set; } = "New"; // New, Good, Fair, Poor, Needs Replacement

        public DateTime? LastServiceDate { get; set; }
        public DateTime? NextServiceDate { get; set; }

        [StringLength(100)]
        public string CurrentLocation { get; set; } = string.Empty;

        public int? AssignedToJobId { get; set; }
        [ForeignKey("AssignedToJobId")]
        public virtual Job? AssignedToJob { get; set; }

        public int? AssignedToTechnicianId { get; set; }
        [ForeignKey("AssignedToTechnicianId")]
        public virtual Technician? AssignedToTechnician { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        // Category relationship
        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual EquipmentCategory? Category { get; set; }

        // Navigation properties for new workflow
        public virtual ICollection<EquipmentLine> EquipmentLines { get; set; } = new List<EquipmentLine>();
        public virtual ICollection<EquipmentInventoryRecord> InventoryRecords { get; set; } = new List<EquipmentInventoryRecord>();
        public virtual ICollection<SiteEquipmentLedger> LedgerEntries { get; set; } = new List<SiteEquipmentLedger>();
        public virtual ICollection<SiteEquipmentSnapshot> SnapshotEntries { get; set; } = new List<SiteEquipmentSnapshot>();
    }
} 