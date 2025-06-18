using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class MaterialInventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; } = null!;

        [Required]
        public double Quantity { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}