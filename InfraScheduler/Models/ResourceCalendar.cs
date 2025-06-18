using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class ResourceCalendar
    {
        public ResourceCalendar()
        {
            Notes = string.Empty;
        }

        [Key]
        public int Id { get; set; }

        [ForeignKey("Technician")]
        public int TechnicianId { get; set; }
        public Technician Technician { get; set; } = null!;

        public DateTime Date { get; set; }
        public bool IsAvailable { get; set; } = true;

        public string Notes { get; set; }

        [NotMapped]
        public string Tooltip => $"{Technician.FirstName} {Technician.LastName}\n" +
                               $"Date: {Date:yyyy-MM-dd}\n" +
                               $"Status: {(IsAvailable ? "Available" : "Unavailable")}\n" +
                               $"Notes: {Notes}";

        public bool IsWeekend => Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;

        public bool IsHoliday => !IsAvailable && string.IsNullOrWhiteSpace(Notes);

        public bool IsLeave => !IsAvailable && Notes.Contains("leave", StringComparison.OrdinalIgnoreCase);

        public bool IsTraining => !IsAvailable && Notes.Contains("training", StringComparison.OrdinalIgnoreCase);

        public bool IsMaintenance => !IsAvailable && Notes.Contains("maintenance", StringComparison.OrdinalIgnoreCase);

        public bool IsEmergency => !IsAvailable && Notes.Contains("emergency", StringComparison.OrdinalIgnoreCase);
    }
}
