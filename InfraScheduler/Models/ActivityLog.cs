using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty;

        [Required]
        public string EntityAffected { get; set; } = string.Empty;

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
