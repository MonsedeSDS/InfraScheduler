using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class Technician
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        public string Roles { get; set; } = string.Empty;

        public string Certifications { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public decimal HourlyRate { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
