using System;
using System.ComponentModel.DataAnnotations;

namespace InfraScheduler.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string ContactPerson { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;


        // Navigation property: one client can be tenant to many sites
        public ICollection<SiteTenant> SiteTenants { get; set; } = new List<SiteTenant>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
