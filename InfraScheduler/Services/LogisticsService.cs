using InfraScheduler.Data;
using InfraScheduler.Models.EquipmentManagement;
using Microsoft.EntityFrameworkCore;

namespace InfraScheduler.Services
{
    public class LogisticsService
    {
        private readonly InfraSchedulerContext _context;

        public LogisticsService(InfraSchedulerContext context)
        {
            _context = context;
        }

        public async Task MarkShipped(int batchId)
        {
            var batch = await _context.EquipmentBatches
                .Include(b => b.Lines)
                .FirstOrDefaultAsync(b => b.Id == batchId);

            if (batch == null)
                throw new ArgumentException($"Equipment batch with ID {batchId} not found");

            // Mark batch as shipped
            batch.Status = "Shipped";

            // Update all lines in the batch
            foreach (var line in batch.Lines)
            {
                line.Status = EquipmentStatus.OnSiteInstalling;
                line.ShippedDate = DateTime.UtcNow;

                // Update inventory location from SDSWarehouse to Site
                var inventoryRecord = await _context.EquipmentInventoryRecords
                    .FirstOrDefaultAsync(r => r.EquipmentTypeId == line.EquipmentTypeId && 
                                            r.Location == InventoryLocation.SDSWarehouse &&
                                            r.SiteId == batch.SiteId);

                if (inventoryRecord != null)
                {
                    inventoryRecord.Location = InventoryLocation.Site;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
} 