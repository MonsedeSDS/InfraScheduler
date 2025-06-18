using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class Allocation
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("JobTask")]
        public int JobTaskId { get; set; }
        public JobTask JobTask { get; set; } = null!;

        [ForeignKey("Technician")]
        public int TechnicianId { get; set; }
        public Technician Technician { get; set; } = null!;

        public DateTime AllocationDate { get; set; }
        public int HoursAllocated { get; set; }
    }
}
