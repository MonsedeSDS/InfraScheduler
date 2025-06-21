using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class InstallationLog
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
        public DateTime CompletionDate { get; set; }

        [StringLength(2000)]
        public string Summary { get; set; } = string.Empty;

        [StringLength(5000)]
        public string TechnicalNotes { get; set; } = string.Empty;

        [StringLength(500)]
        public string PDFReportPath { get; set; } = string.Empty; // File path to generated PDF

        [StringLength(100)]
        public string CompletedBy { get; set; } = string.Empty; // User who closed the job

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Technical details
        public int TotalEquipmentInstalled { get; set; }
        public int TotalTechniciansInvolved { get; set; }
        public TimeSpan TotalLaborHours { get; set; }

        [StringLength(1000)]
        public string EquipmentList { get; set; } = string.Empty; // JSON or comma-separated list of installed equipment
    }
} 