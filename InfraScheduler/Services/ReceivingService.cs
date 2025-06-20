using InfraScheduler.Data;
using InfraScheduler.Models.EquipmentManagement;
using Microsoft.EntityFrameworkCore;

namespace InfraScheduler.Services
{
    public class ReceivingService
    {
        private readonly InfraSchedulerContext _context;

        public ReceivingService(InfraSchedulerContext context)
        {
            _context = context;
        }

        public async Task ReceiveLine(int lineId, int receivedQty)
        {
            var line = await _context.EquipmentLines
                .Include(l => l.Batch)
                .FirstOrDefaultAsync(l => l.Id == lineId);

            if (line == null)
                throw new ArgumentException($"Equipment line with ID {lineId} not found");

            // Update quantities
            line.ReceivedQty = receivedQty;
            line.ReceivedDate = DateTime.UtcNow;
            line.Status = EquipmentStatus.SDSWarehouse;

            // Log discrepancy if there's a difference
            if (receivedQty != line.PlannedQty)
            {
                var discrepancy = new EquipmentDiscrepancy
                {
                    LineId = lineId,
                    PlannedQty = line.PlannedQty,
                    ReceivedQty = receivedQty,
                    Note = $"Planned: {line.PlannedQty}, Received: {receivedQty}"
                };

                _context.EquipmentDiscrepancies.Add(discrepancy);
            }

            // Create or update inventory record
            var inventoryRecord = await _context.EquipmentInventoryRecords
                .FirstOrDefaultAsync(r => r.EquipmentTypeId == line.EquipmentTypeId && 
                                        r.Location == InventoryLocation.SDSWarehouse);

            if (inventoryRecord == null)
            {
                inventoryRecord = new EquipmentInventoryRecord
                {
                    EquipmentTypeId = line.EquipmentTypeId,
                    Quantity = receivedQty,
                    Location = InventoryLocation.SDSWarehouse,
                    ReservedForSite = true,
                    SiteId = line.Batch.SiteId
                };
                _context.EquipmentInventoryRecords.Add(inventoryRecord);
            }
            else
            {
                inventoryRecord.Quantity += receivedQty;
                inventoryRecord.ReservedForSite = true;
                inventoryRecord.SiteId = line.Batch.SiteId;
            }

            await _context.SaveChangesAsync();
        }
    }
} 