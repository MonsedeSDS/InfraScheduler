using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Models.EquipmentManagement;
using InfraScheduler.Services;
using Microsoft.EntityFrameworkCore;

namespace InfraScheduler.Services
{
    public class WorkflowOrchestrator
    {
        private readonly InfraSchedulerContext _context;
        private readonly JobService _jobService;
        private readonly ReceivingService _receivingService;
        private readonly LogisticsService _logisticsService;
        private readonly TaskService _taskService;
        private readonly EquipmentService _equipmentService;
        private readonly SiteEquipmentService _siteEquipmentService;

        public WorkflowOrchestrator(InfraSchedulerContext context)
        {
            _context = context;
            _jobService = new JobService(context);
            _receivingService = new ReceivingService(context);
            _logisticsService = new LogisticsService(context);
            _taskService = new TaskService(context);
            _equipmentService = new EquipmentService(context);
            _siteEquipmentService = new SiteEquipmentService(context);
        }

        /// <summary>
        /// Step 1: Job Acceptance - PM accepts job and creates equipment batch
        /// </summary>
        public async Task<EquipmentBatch> AcceptJob(int jobId)
        {
            try
            {
                // Validate job exists and is in correct status
                var job = await _context.Jobs
                    .Include(j => j.Requirements)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job == null)
                    throw new ArgumentException($"Job with ID {jobId} not found");

                if (job.Status != "Pending" && job.Status != "Created")
                    throw new InvalidOperationException($"Job must be in 'Pending' or 'Created' status to be accepted");

                if (!job.Requirements.Any())
                    throw new InvalidOperationException($"Job must have equipment requirements before acceptance");

                // Create equipment batch
                var batch = await _jobService.AcceptJob(jobId);

                // Update job status
                job.Status = "Accepted";
                await _context.SaveChangesAsync();

                return batch;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error accepting job: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Step 2: Manual Receiving - User receives equipment at SDS warehouse
        /// </summary>
        public async Task ReceiveEquipmentBatch(int batchId, Dictionary<int, int> lineQuantities)
        {
            try
            {
                var batch = await _context.EquipmentBatches
                    .Include(b => b.Lines)
                    .FirstOrDefaultAsync(b => b.Id == batchId);

                if (batch == null)
                    throw new ArgumentException($"Equipment batch with ID {batchId} not found");

                if (batch.Status != "Created")
                    throw new InvalidOperationException($"Batch must be in 'Created' status for receiving");

                // Receive each line
                foreach (var kvp in lineQuantities)
                {
                    var lineId = kvp.Key;
                    var receivedQty = kvp.Value;

                    await _receivingService.ReceiveLine(lineId, receivedQty);
                }

                // Update batch status if all lines are received
                var allReceived = batch.Lines.All(l => l.Status == EquipmentStatus.SDSWarehouse);
                if (allReceived)
                {
                    batch.Status = "Ready for Shipping";
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error receiving equipment batch: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Step 3: Dispatch to Site - Logistics marks batch as shipped
        /// </summary>
        public async Task ShipEquipmentBatch(int batchId)
        {
            try
            {
                var batch = await _context.EquipmentBatches
                    .Include(b => b.Lines)
                    .FirstOrDefaultAsync(b => b.Id == batchId);

                if (batch == null)
                    throw new ArgumentException($"Equipment batch with ID {batchId} not found");

                if (batch.Status != "Ready for Shipping")
                    throw new InvalidOperationException($"Batch must be in 'Ready for Shipping' status");

                // Verify all lines are in SDS warehouse
                var notReceived = batch.Lines.Where(l => l.Status != EquipmentStatus.SDSWarehouse);
                if (notReceived.Any())
                    throw new InvalidOperationException($"All equipment lines must be received before shipping");

                // Mark as shipped
                await _logisticsService.MarkShipped(batchId);

                // Update batch status
                batch.Status = "Shipped";
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error shipping equipment batch: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Step 4: Field Installation - Track equipment installation through task completion
        /// </summary>
        public async Task CompleteTaskWithEquipment(int taskId)
        {
            try
            {
                var task = await _context.JobTasks
                    .Include(t => t.JobTaskEquipmentLines)
                    .ThenInclude(jtel => jtel.EquipmentLine)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                    throw new ArgumentException($"Task with ID {taskId} not found");

                // Mark task as complete
                await _taskService.MarkTaskComplete(taskId);

                // Check if any equipment lines should be marked as installed
                foreach (var taskEquipmentLine in task.JobTaskEquipmentLines)
                {
                    var equipmentLine = taskEquipmentLine.EquipmentLine;
                    
                    // Check if this is the last task using this equipment line
                    var remainingTasks = await _context.JobTaskEquipmentLines
                        .Where(jtel => jtel.EquipmentLineId == equipmentLine.Id && 
                                      jtel.JobTask.Status != "Completed")
                        .CountAsync();

                    if (remainingTasks == 0) // This was the last task using this equipment line
                    {
                        await _equipmentService.MarkInstalled(equipmentLine.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error completing task: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Step 5: Job Close-Out - PM closes job and generates reports
        /// </summary>
        public async Task CloseJobWithValidation(int jobId)
        {
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Site)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job == null)
                    throw new ArgumentException($"Job with ID {jobId} not found");

                if (job.Status != "Accepted" && job.Status != "In Progress")
                    throw new InvalidOperationException($"Job must be in 'Accepted' or 'In Progress' status to be closed");

                // Get equipment batch
                var batch = await _context.EquipmentBatches
                    .Include(b => b.Lines)
                    .FirstOrDefaultAsync(b => b.JobId == jobId);

                if (batch == null)
                    throw new ArgumentException($"No equipment batch found for job {jobId}");

                // Validate all equipment lines are installed
                var uninstalledLines = batch.Lines.Where(l => l.Status != EquipmentStatus.OnSiteInstalled);
                if (uninstalledLines.Any())
                {
                    var equipmentNames = string.Join(", ", uninstalledLines.Select(l => l.EquipmentType.Name));
                    throw new InvalidOperationException($"Cannot close job: The following equipment is not installed: {equipmentNames}");
                }

                // Close the job
                await _jobService.CloseJob(jobId);

                // Update job status
                job.Status = "Completed";
                job.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error closing job: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Step 6: Permanent Site Ledger Update - Update site equipment ledger
        /// </summary>
        public async Task UpdateSiteEquipmentLedger(int batchId, int jobId)
        {
            try
            {
                await _siteEquipmentService.AppendLedgerEntries(batchId, jobId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating site equipment ledger: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Step 7: Nightly Snapshot Rebuild - Rebuild site equipment snapshots
        /// </summary>
        public async Task RebuildSiteEquipmentSnapshots()
        {
            try
            {
                await _siteEquipmentService.RebuildSnapshot();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error rebuilding site equipment snapshots: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get workflow status for a job
        /// </summary>
        public async Task<WorkflowStatus> GetWorkflowStatus(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Site)
                .Include(j => j.Client)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
                throw new ArgumentException($"Job with ID {jobId} not found");

            var batch = await _context.EquipmentBatches
                .Include(b => b.Lines)
                .ThenInclude(l => l.EquipmentType)
                .FirstOrDefaultAsync(b => b.JobId == jobId);

            var status = new WorkflowStatus
            {
                JobId = jobId,
                JobName = job.Name,
                JobStatus = job.Status,
                SiteName = job.Site.SiteName,
                ClientName = job.Client.Name,
                BatchStatus = batch?.Status ?? "No Batch",
                TotalEquipmentLines = batch?.Lines.Count ?? 0,
                ReceivedLines = batch?.Lines.Count(l => l.Status == EquipmentStatus.SDSWarehouse) ?? 0,
                ShippedLines = batch?.Lines.Count(l => l.Status == EquipmentStatus.OnSiteInstalling) ?? 0,
                InstalledLines = batch?.Lines.Count(l => l.Status == EquipmentStatus.OnSiteInstalled) ?? 0,
                CanBeAccepted = job.Status == "Pending" || job.Status == "Created",
                CanBeReceived = batch?.Status == "Created",
                CanBeShipped = batch?.Status == "Ready for Shipping",
                CanBeClosed = job.Status == "Accepted" || job.Status == "In Progress"
            };

            return status;
        }
    }

    public class WorkflowStatus
    {
        public int JobId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public string JobStatus { get; set; } = string.Empty;
        public string SiteName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string BatchStatus { get; set; } = string.Empty;
        public int TotalEquipmentLines { get; set; }
        public int ReceivedLines { get; set; }
        public int ShippedLines { get; set; }
        public int InstalledLines { get; set; }
        public bool CanBeAccepted { get; set; }
        public bool CanBeReceived { get; set; }
        public bool CanBeShipped { get; set; }
        public bool CanBeClosed { get; set; }
    }
} 