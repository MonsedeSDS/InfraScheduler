using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfraScheduler.Services.Interfaces;

namespace InfraScheduler.Services
{
    public class TechnicianAvailability : ITechnicianAvailability
    {
        public IEnumerable<TechnicianAvailabilityDto> GetTechnicianAvailability(
            int techId, DateTime startDate, DateTime endDate)
            => Enumerable.Empty<TechnicianAvailabilityDto>();

        public Task<bool> CheckAvailabilityAsync(
            int techId, DateTime startDate, DateTime endDate)
            => Task.FromResult(true);
    }
} 