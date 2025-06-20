using InfraScheduler.Data;
using InfraScheduler.Models.EquipmentManagement;
using Microsoft.EntityFrameworkCore;

namespace InfraScheduler.Services
{
    public class SiteEquipmentQuery
    {
        private readonly InfraSchedulerContext _context;

        public SiteEquipmentQuery(InfraSchedulerContext context)
        {
            _context = context;
        }

        public async Task<List<SiteEquipmentSnapshot>> GetCurrentBySite(int siteId)
        {
            return await _context.SiteEquipmentSnapshots
                .Include(s => s.Site)
                .Include(s => s.EquipmentType)
                .Where(s => s.SiteId == siteId)
                .ToListAsync();
        }
    }
} 