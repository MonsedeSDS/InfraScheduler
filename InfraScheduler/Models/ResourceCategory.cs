using System.ComponentModel.DataAnnotations;

namespace InfraScheduler.Models
{
    public class ResourceCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
    }
}
