using System.ComponentModel.DataAnnotations;

namespace InfraScheduler.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
} 