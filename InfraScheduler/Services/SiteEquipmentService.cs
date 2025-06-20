using InfraScheduler.Data;
using InfraScheduler.Models.EquipmentManagement;
using Microsoft.EntityFrameworkCore;

namespace InfraScheduler.Services
{
    public class SiteEquipmentService
    {
        private readonly InfraSchedulerContext _context;

        public SiteEquipmentService(InfraSchedulerContext context)
        {
            _context = context;
        }

        public async Task AppendLedgerEntries(int batchId, int jobId)
        {
            var batch = await _context.EquipmentBatches
                .Include(b => b.Lines)
                .ThenInclude(l => l.EquipmentType)
                .FirstOrDefaultAsync(b => b.Id == batchId);

            if (batch == null)
                throw new ArgumentException($"Equipment batch with ID {batchId} not found");

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
                throw new ArgumentException($"Job with ID {jobId} not found");

            foreach (var line in batch.Lines.Where(l => l.Status == EquipmentStatus.OnSiteInstalled))
            {
                var ledgerEntry = new SiteEquipmentLedger
                {
                    SiteId = batch.SiteId,
                    EquipmentTypeId = line.EquipmentTypeId,
                    QuantityInstalled = line.ReceivedQty, // Positive for installations
                    InstallationDate = line.InstalledDate ?? DateTime.UtcNow,
                    SourceJobId = jobId,
                    BatchId = batchId,
                    LineId = line.Id,
                    SerialNumbers = "" // TODO: Implement serial number tracking
                };

                _context.SiteEquipmentLedgers.Add(ledgerEntry);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RebuildSnapshot()
        {
            // Clear existing snapshots
            _context.SiteEquipmentSnapshots.RemoveRange(_context.SiteEquipmentSnapshots);
            await _context.SaveChangesAsync();

            // Get all unique site-equipment combinations from ledger
            var siteEquipmentGroups = await _context.SiteEquipmentLedgers
                .GroupBy(l => new { l.SiteId, l.EquipmentTypeId })
                .Select(g => new
                {
                    g.Key.SiteId,
                    g.Key.EquipmentTypeId,
                    CurrentQty = g.Sum(l => l.QuantityInstalled)
                })
                .ToListAsync();

            // Create new snapshots
            foreach (var group in siteEquipmentGroups)
            {
                var snapshot = new SiteEquipmentSnapshot
                {
                    SiteId = group.SiteId,
                    EquipmentTypeId = group.EquipmentTypeId,
                    CurrentQty = group.CurrentQty,
                    LastUpdateUtc = DateTime.UtcNow
                };

                _context.SiteEquipmentSnapshots.Add(snapshot);
            }

            await _context.SaveChangesAsync();
        }
    }
} 