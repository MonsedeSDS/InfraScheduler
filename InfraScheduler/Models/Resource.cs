using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class Resource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public ResourceCategory Category { get; set; } = null!;

        public ICollection<ResourceCalendar> Calendar { get; set; } = new List<ResourceCalendar>();
        public ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
    }
}
