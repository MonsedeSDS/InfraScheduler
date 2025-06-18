using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfraScheduler.Models;

namespace InfraScheduler.Services.Interfaces
{
    public interface ITechnicianAvailability
    {
        IEnumerable<TechnicianAvailabilityDto> GetTechnicianAvailability(
            int techId, DateTime startDate, DateTime endDate);

        Task<bool> CheckAvailabilityAsync(
            int techId, DateTime startDate, DateTime endDate);
    }

    public class TechnicianAvailability
    {
        public bool IsAvailable { get; set; }
        public DateTime EarliestAvailableDate { get; set; }
    }
} 