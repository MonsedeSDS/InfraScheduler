using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class SiteTenant
    {
        [Key]
        public int Id { get; set; }

        public int SiteId { get; set; }
        [ForeignKey("SiteId")]
        public Site Site { get; set; } = null!;

        public int ClientId { get; set; }
        [ForeignKey("ClientId")]
        public Client Client { get; set; } = null!;
    }
}
