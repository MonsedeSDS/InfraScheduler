using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class TaskDependency
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ParentTaskId { get; set; }

        [ForeignKey("ParentTaskId")]
        public JobTask ParentTask { get; set; } = null!;

        [Required]
        public int PrerequisiteTaskId { get; set; }

        [ForeignKey("PrerequisiteTaskId")]
        public JobTask PrerequisiteTask { get; set; } = null!;
    }
}
