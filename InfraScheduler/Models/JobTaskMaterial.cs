using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class JobTaskMaterial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JobTaskId { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [ForeignKey("JobTaskId")]
        public virtual JobTask? JobTask { get; set; }

        [ForeignKey("MaterialId")]
        public virtual Material? Material { get; set; }
    }
} 