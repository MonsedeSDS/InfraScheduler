using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class TechnicianAssignment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("JobTask")]
        public int JobTaskId { get; set; }
        public JobTask JobTask { get; set; } = null!;

        [ForeignKey("Technician")]
        public int TechnicianId { get; set; }
        public Technician Technician { get; set; } = null!;
    }
}
