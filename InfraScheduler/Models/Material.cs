using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class Tool
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
        public virtual ToolCategory? Category { get; set; }

        // Navigation properties
        public ICollection<ToolAssignment> Assignments { get; set; } = new List<ToolAssignment>();
        public ICollection<ToolMaintenance> MaintenanceHistory { get; set; } = new List<ToolMaintenance>();
    }
}
