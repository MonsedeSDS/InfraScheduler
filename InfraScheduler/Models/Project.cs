using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class Project
    {
        public Project()
        {
            Jobs = new List<Job>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int ClientId { get; set; }
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, On Hold, Completed, Cancelled

        [Required]
        [StringLength(50)]
        public string ProjectNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Job> Jobs { get; set; }

        // Computed properties for UI
        [NotMapped]
        public int JobCount => Jobs?.Count ?? 0;

        [NotMapped]
        public int CompletedJobsCount => Jobs?.Count(j => j.Status == "Completed") ?? 0;

        [NotMapped]
        public double CompletionPercentage => JobCount > 0 ? (double)CompletedJobsCount / JobCount * 100 : 0;

        [NotMapped]
        public string StatusSummary => $"{Status} • {JobCount} Jobs • {CompletionPercentage:F0}% Complete";

        [NotMapped]
        public DateTime? NextDeadline => Jobs?.Where(j => j.EndDate.HasValue && j.Status != "Completed")
                                              .Min(j => j.EndDate);
    }
} 