using System;

namespace InfraScheduler.Services.Interfaces
{
    public class TechnicianAvailabilityDto
    {
        public int TechnicianId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAvailable { get; set; }
    }
} 