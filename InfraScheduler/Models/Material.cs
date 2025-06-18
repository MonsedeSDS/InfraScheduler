using System.ComponentModel.DataAnnotations;

namespace InfraScheduler.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Notes { get; set; } = string.Empty;
        public ICollection<MaterialRequirement> MaterialRequirements { get; set; } = new List<MaterialRequirement>();
    }
}
