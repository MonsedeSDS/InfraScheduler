using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class ToolAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ToolId { get; set; }

        public int? TechnicianId { get; set; }
        public int? JobId { get; set; }

        [Required]
        public DateTime CheckoutDate { get; set; }

        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Checked Out"; // Checked Out, Returned, Overdue

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        [ForeignKey("ToolId")]
        public virtual Tool Tool { get; set; } = null!;

        [ForeignKey("TechnicianId")]
        public virtual Technician? Technician { get; set; }

        [ForeignKey("JobId")]
        public virtual Job? Job { get; set; }
    }
} 