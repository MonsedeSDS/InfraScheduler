using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InfraScheduler.Services.Interfaces
{
    public interface IMaterialAvailability
    {
        IEnumerable<MaterialAvailabilityDto> GetMaterialAvailability(
            int materialId, DateTime startDate, DateTime endDate);

        Task<bool> CheckAvailabilityAsync(
            int materialId, int requiredQuantity,
            DateTime startDate, DateTime endDate);
    }

    public class MaterialAvailabilityDto
    {
        public int MaterialId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AvailableQuantity { get; set; }
    }
} 