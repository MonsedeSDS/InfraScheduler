using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Models.EquipmentManagement;
using Microsoft.EntityFrameworkCore;

namespace InfraScheduler.Services
{
    public class JobService
    {
        private readonly InfraSchedulerContext _context;

        public JobService(InfraSchedulerContext context)
        {
            _context = context;
        }

        public async Task<EquipmentBatch> AcceptJob(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Site)
                .Include(j => j.Requirements)
                .ThenInclude(r => r.EquipmentType)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
                throw new ArgumentException($"Job with ID {jobId} not found");

            // Create EquipmentBatch
            var batch = new EquipmentBatch
            {
                JobId = jobId,
                SiteId = job.SiteId,
                Status = "Created"
            };

            _context.EquipmentBatches.Add(batch);
            await _context.SaveChangesAsync();

            // Create EquipmentLine entries from job requirements
            foreach (var requirement in job.Requirements)
            {
                var equipmentLine = new EquipmentLine
                {
                    BatchId = batch.Id,
                    EquipmentTypeId = requirement.EquipmentTypeId,
                    PlannedQty = requirement.PlannedQty,
                    Status = EquipmentStatus.ClientWarehouse
                };

                _context.EquipmentLines.Add(equipmentLine);
            }

            await _context.SaveChangesAsync();

            return batch;
        }

        public async Task CloseJob(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Site)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
                throw new ArgumentException($"Job with ID {jobId} not found");

            // Get all equipment lines for this job
            var batch = await _context.EquipmentBatches
                .Include(b => b.Lines)
                .FirstOrDefaultAsync(b => b.JobId == jobId);

            if (batch == null)
                throw new ArgumentException($"No equipment batch found for job {jobId}");

            // Validate all lines are installed
            var uninstalledLines = batch.Lines.Where(l => l.Status != EquipmentStatus.OnSiteInstalled);
            if (uninstalledLines.Any())
            {
                throw new InvalidOperationException($"Cannot close job: {uninstalledLines.Count()} equipment lines are not installed");
            }

            // Move inventory to SitePermanent
            foreach (var line in batch.Lines)
            {
                var inventoryRecord = await _context.EquipmentInventoryRecords
                    .FirstOrDefaultAsync(r => r.EquipmentTypeId == line.EquipmentTypeId && r.SiteId == job.SiteId);

                if (inventoryRecord != null)
                {
                    inventoryRecord.Location = InventoryLocation.SitePermanent;
                }
            }

            // Lock the batch
            batch.Status = "Closed";

            await _context.SaveChangesAsync();

            // Generate PDF report and email (placeholder for now)
            // TODO: Implement PDF generation and email functionality

            // Append ledger entries
            var siteEquipmentService = new SiteEquipmentService(_context);
            await siteEquipmentService.AppendLedgerEntries(batch.Id, jobId);
        }

        public async Task AssignEquipmentToTask(int taskId, int equipmentLineId, int quantity = 1, string notes = "")
        {
            var task = await _context.JobTasks.FindAsync(taskId);
            if (task == null)
                throw new ArgumentException($"Task with ID {taskId} not found");

            var equipmentLine = await _context.EquipmentLines.FindAsync(equipmentLineId);
            if (equipmentLine == null)
                throw new ArgumentException($"Equipment line with ID {equipmentLineId} not found");

            // Check if assignment already exists
            var existingAssignment = await _context.JobTaskEquipmentLines
                .FirstOrDefaultAsync(jtel => jtel.JobTaskId == taskId && jtel.EquipmentLineId == equipmentLineId);

            if (existingAssignment != null)
            {
                // Update existing assignment
                existingAssignment.Quantity = quantity;
                existingAssignment.Notes = notes;
            }
            else
            {
                // Create new assignment
                var assignment = new JobTaskEquipmentLine
                {
                    JobTaskId = taskId,
                    EquipmentLineId = equipmentLineId,
                    Quantity = quantity,
                    Notes = notes
                };

                _context.JobTaskEquipmentLines.Add(assignment);
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddJobRequirement(int jobId, int equipmentTypeId, int plannedQty, string description = "", string priority = "Normal", string notes = "")
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
                throw new ArgumentException($"Job with ID {jobId} not found");

            var equipmentType = await _context.Equipment.FindAsync(equipmentTypeId);
            if (equipmentType == null)
                throw new ArgumentException($"Equipment type with ID {equipmentTypeId} not found");

            var requirement = new JobRequirement
            {
                JobId = jobId,
                EquipmentTypeId = equipmentTypeId,
                PlannedQty = plannedQty,
                Description = description,
                Priority = priority,
                Notes = notes
            };

            _context.JobRequirements.Add(requirement);
            await _context.SaveChangesAsync();
        }
    }
} 