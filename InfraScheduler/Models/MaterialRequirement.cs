using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class MaterialRequirement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JobTaskId { get; set; }

        [ForeignKey("JobTaskId")]
        public virtual JobTask JobTask { get; set; } = null!;

        [Required]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; } = null!;

        [Required]
        public double Quantity { get; set; }

        [Required]
        public string Unit { get; set; } = string.Empty;
    }
}