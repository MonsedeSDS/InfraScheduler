using System;

namespace InfraScheduler.Models
{
    public class MaterialRequirement
    {
        public int Id { get; set; }
        public int JobTaskId { get; set; }
        public JobTask JobTask { get; set; }
        public DateTime RequiredFrom { get; set; }
        public DateTime RequiredTo { get; set; }
    }
} 