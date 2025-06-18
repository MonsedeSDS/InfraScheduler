using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfraScheduler.Services.Interfaces;

namespace InfraScheduler.Services
{
    public class MaterialAvailability : IMaterialAvailability
    {
        public int MaterialId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Quantity { get; set; }
        public bool IsReserved { get; set; }

        public IEnumerable<MaterialAvailabilityDto> GetMaterialAvailability(
            int materialId, DateTime startDate, DateTime endDate)
            => Enumerable.Empty<MaterialAvailabilityDto>();

        public Task<bool> CheckAvailabilityAsync(
            int materialId, int requiredQuantity,
            DateTime startDate, DateTime endDate)
            => Task.FromResult(true);
    }
} 