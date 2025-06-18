using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class JobTask
    {
        public JobTask()
        {
            MaterialRequirements = new List<MaterialRequirement>();
            Dependencies = new List<TaskDependency>();
            Name = string.Empty;
            Description = string.Empty;
            Status = string.Empty;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, 100)]
        public double Progress { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        [ForeignKey("Job")]
        public int JobId { get; set; }
        public virtual Job Job { get; set; } = null!;

        [ForeignKey("Technician")]
        public int? TechnicianId { get; set; }
        public virtual Technician? Technician { get; set; }

        public virtual ICollection<MaterialRequirement> MaterialRequirements { get; set; }
        public virtual ICollection<TaskDependency> Dependencies { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? PlannedStart { get; set; }
        public DateTime? PlannedEnd { get; set; }

        // Alias for Name to maintain compatibility
        [NotMapped]
        public string TaskName
        {
            get => Name;
            set => Name = value;
        }

        [NotMapped]
        public double Duration => (EndDate - StartDate).TotalDays;

        [NotMapped]
        public string Tooltip => $"{Name}\nStart: {StartDate:yyyy-MM-dd}\nEnd: {EndDate:yyyy-MM-dd}\nProgress: {Progress:P0}";

        // Alias for TechnicianId to maintain compatibility
        [NotMapped]
        public int? AssignedTechnicianId
        {
            get => TechnicianId;
            set => TechnicianId = value;
        }

        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
