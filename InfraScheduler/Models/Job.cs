using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class Job
    {
        public Job()
        {
            Tasks = new List<JobTask>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public virtual ICollection<JobTask> Tasks { get; set; }

        // Navigation property for job requirements
        public virtual ICollection<JobRequirement> Requirements { get; set; } = new List<JobRequirement>();

        [ForeignKey("Site")]
        public int SiteId { get; set; }
        public Site Site { get; set; } = null!;

        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(50)]
        public string JobNumber { get; set; }

        [StringLength(50)]
        public string JobType { get; set; }

        [NotMapped]
        public string JobName
        {
            get => Name;
            set => Name = value;
        }
    }
}
