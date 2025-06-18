using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class JobTaskTechnician
    {
        [Key]
        public int Id { get; set; }

        public int JobTaskId { get; set; }
        [ForeignKey("JobTaskId")]
        public JobTask JobTask { get; set; } = null!;

        public int TechnicianId { get; set; }
        [ForeignKey("TechnicianId")]
        public Technician Technician { get; set; } = null!;
    }
}
