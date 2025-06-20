using InfraScheduler.Data;
using InfraScheduler.Models.EquipmentManagement;
using Microsoft.EntityFrameworkCore;

namespace InfraScheduler.Services
{
    public class EquipmentService
    {
        private readonly InfraSchedulerContext _context;

        public EquipmentService(InfraSchedulerContext context)
        {
            _context = context;
        }

        public async Task MarkInstalled(int lineId)
        {
            var line = await _context.EquipmentLines
                .FirstOrDefaultAsync(l => l.Id == lineId);

            if (line == null)
                throw new ArgumentException($"Equipment line with ID {lineId} not found");

            // Set installed date and status
            line.InstalledDate = DateTime.UtcNow;
            line.Status = EquipmentStatus.OnSiteInstalled;

            await _context.SaveChangesAsync();
        }
    }
} 