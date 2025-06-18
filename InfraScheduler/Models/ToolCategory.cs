using System.ComponentModel.DataAnnotations;

namespace InfraScheduler.Models
{
    public class ToolCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<Tool> Tools { get; set; } = new List<Tool>();
    }
} 