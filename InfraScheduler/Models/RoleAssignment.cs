using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class RoleAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TechnicianId { get; set; }

        [ForeignKey("TechnicianId")]
        public Technician Technician { get; set; } = null!;

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; } = null!;
    }
} 