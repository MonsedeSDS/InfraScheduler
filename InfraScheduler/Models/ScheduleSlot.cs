using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfraScheduler.Models
{
    public class ScheduleSlot
    {
        public ScheduleSlot()
        {
            Notes = string.Empty;
        }

        [Key]
        public int Id { get; set; }

        [ForeignKey("JobTask")]
        public int JobTaskId { get; set; }
        public JobTask JobTask { get; set; } = null!;

        [ForeignKey("Technician")]
        public int TechnicianId { get; set; }
        public Technician Technician { get; set; } = null!;

        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }

        public bool IsLocked { get; set; } = false; // prevents accidental reschedule
        public string Notes { get; set; }

        [NotMapped]
        public TimeSpan Duration => ScheduledEnd - ScheduledStart;

        [NotMapped]
        public string Tooltip => $"{JobTask.Name}\n" +
                               $"Technician: {Technician.FirstName} {Technician.LastName}\n" +
                               $"Start: {ScheduledStart:yyyy-MM-dd HH:mm}\n" +
                               $"End: {ScheduledEnd:yyyy-MM-dd HH:mm}\n" +
                               $"Duration: {Duration.TotalHours:F1} hours\n" +
                               $"Status: {(IsLocked ? "Locked" : "Unlocked")}\n" +
                               $"Notes: {Notes}";

        public bool HasOverlap(ScheduleSlot other)
        {
            return ScheduledStart < other.ScheduledEnd && other.ScheduledStart < ScheduledEnd;
        }

        public bool IsWithinDateRange(DateTime start, DateTime end)
        {
            return ScheduledStart >= start && ScheduledEnd <= end;
        }

        public bool IsOnDate(DateTime date)
        {
            return ScheduledStart.Date <= date && ScheduledEnd.Date >= date;
        }
    }
}
