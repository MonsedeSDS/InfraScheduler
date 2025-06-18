using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Net.Mail;
using System.Linq;

namespace InfraScheduler.Models
{
    public class Site
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string SiteName { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        public string SiteCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // FK to SiteOwner
        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        [ForeignKey("SiteOwner")]
        public int SiteOwnerId { get; set; }
        public SiteOwner SiteOwner { get; set; } = null!;

        public ICollection<SiteTenant> SiteTenants { get; set; } = new List<SiteTenant>();

        [NotMapped]
        public string TenantList => string.Join(", ", SiteTenants.Select(t => t.Client.Name));
    }
}
