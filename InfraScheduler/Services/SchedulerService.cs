using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace InfraScheduler.Services
{
    public interface ISchedulerService
    {
        Task<List<string>> ScheduleAllUnassignedJobsAsync();
        Task<List<string>> AnalyzeTaskAsync(JobTask task);
        Task<bool> AssignTechnicianAsync(JobTask task, int technicianId);
        Task<bool> UnassignTechnicianAsync(JobTask task);
        Task<Technician?> FindEarliestAvailableTechnicianAsync(JobTask task);
        Task<List<ResourceConflict>> DetectConflictsAsync(JobTask task);
        Task<List<JobTask>> GetTasksForPeriodAsync(DateTime startDate, DateTime endDate);
        Task<List<JobTask>> GetCriticalPathAsync(int jobId);
        Task<List<SchedulingSuggestion>> GetSchedulingSuggestionsAsync(JobTask task);
        Task<bool> ValidateScheduleAsync(JobTask task);
        Task<DateTime?> FindOptimalStartDateAsync(JobTask task);
        Task<bool> LockSlotAsync(ScheduleSlot slot);
        Task<bool> UnlockSlotAsync(ScheduleSlot slot);
    }

    public class SchedulerService : ISchedulerService
    {
        private readonly InfraSchedulerContext _context;
        private readonly ForecastService _forecastService;
        private readonly ConflictService _conflictService;
        private readonly MaterialForecastService _materialForecastService;
        private readonly ResourceLockValidator _lockValidator;

        public SchedulerService(InfraSchedulerContext context)
        {
            _context = context;
            _forecastService = new ForecastService(context);
            _conflictService = new ConflictService(context);
            _materialForecastService = new MaterialForecastService(context);
            _lockValidator = new ResourceLockValidator(context);
        }

        // Schedules all unassigned jobs
        public async Task<List<string>> ScheduleAllUnassignedJobsAsync()
        {
            var results = new List<string>();
            var unassignedJobs = await _context.Jobs
                .Include(j => j.Tasks)
                .Where(j => j.Tasks.All(t => t.TechnicianId == null))
                .ToListAsync();

            foreach (var job in unassignedJobs)
            {
                foreach (var task in job.Tasks)
                {
                    var availableTech = await FindEarliestAvailableTechnicianAsync(task);
                    if (availableTech != null)
                    {
                        task.TechnicianId = availableTech.Id;
                        results.Add($"Job {job.Name} - Task {task.Name} assigned to {availableTech.FirstName} {availableTech.LastName}.");
                    }
                    else
                    {
                        results.Add($"No available technician for Job {job.Name} - Task {task.Name}.");
                    }
                }
            }
            await _context.SaveChangesAsync();
            return results;
        }

        // Analyze a job task for scheduling issues
        public async Task<List<string>> AnalyzeTaskAsync(JobTask task)
        {
            var issues = new List<string>();
            
            // Get forecast issues
            var forecastIssues = await _forecastService.ForecastJobTaskAsync(task);
            issues.AddRange(forecastIssues);

            // Get material forecast issues
            var materialIssues = await Task.Run(() => _materialForecastService.ForecastMaterialForJob(task));
            issues.AddRange(materialIssues);

            // Get conflict issues
            var conflicts = await _conflictService.CheckConflictsAsync(task);
            issues.AddRange(conflicts.Select(c => $"Conflict: {c.Type} - {c.ResourceId}"));

            return issues;
        }

        // Assign a technician to a task if possible
        public async Task<bool> AssignTechnicianAsync(JobTask task, int technicianId)
        {
            var technician = await _context.Technicians.FindAsync(technicianId);
            if (technician == null) return false;

            var conflict = await _context.JobTasks.AnyAsync(t =>
                t.TechnicianId == technicianId &&
                t.StartDate < task.EndDate &&
                t.EndDate > task.StartDate &&
                t.Id != task.Id);

            if (conflict)
                return false;

            task.TechnicianId = technicianId;
            await _context.SaveChangesAsync();
            return true;
        }

        // Unassign a technician from a task
        public async Task<bool> UnassignTechnicianAsync(JobTask task)
        {
            try
            {
                task.TechnicianId = null;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Find earliest available technician for a task
        public async Task<Technician?> FindEarliestAvailableTechnicianAsync(JobTask task)
        {
            var candidates = await _context.Technicians.ToListAsync();
            foreach (var tech in candidates)
            {
                bool available = !await _context.JobTasks.AnyAsync(t =>
                    t.TechnicianId == tech.Id &&
                    t.StartDate < task.EndDate &&
                    t.EndDate > task.StartDate);
                if (available)
                    return tech;
            }
            return null;
        }

        // Check for resource conflicts for a task
        public async Task<List<ResourceConflict>> DetectConflictsAsync(JobTask task)
        {
            return await _conflictService.CheckConflictsAsync(task);
        }

        // Get all tasks for a period
        public async Task<List<JobTask>> GetTasksForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.JobTasks
                .Include(t => t.Job)
                .Include(t => t.Technician)
                .Include(t => t.MaterialRequirements)
                .Where(t => t.StartDate >= startDate && t.EndDate <= endDate)
                .ToListAsync();
        }

        // Get critical path for a job
        public async Task<List<JobTask>> GetCriticalPathAsync(int jobId)
        {
            var criticalPath = await _context.JobTasks
                .Where(t => t.JobId == jobId)
                .Include(t => t.Dependencies)
                .ToListAsync();

            var earliestStart = new Dictionary<int, DateTime>();
            var latestStart = new Dictionary<int, DateTime>();
            var slack = new Dictionary<int, double>();

            // Forward pass
            foreach (var task in criticalPath.OrderBy(t => t.StartDate))
            {
                var maxEarliestStart = task.Dependencies
                    .Where(d => d.ParentTask != null)
                    .Select(d => d.ParentTask.EndDate)
                    .DefaultIfEmpty(task.StartDate)
                    .Max();
                earliestStart[task.Id] = maxEarliestStart;
            }

            // Backward pass
            foreach (var task in criticalPath.OrderByDescending(t => t.EndDate))
            {
                var minLatestStart = task.Dependencies
                    .Where(d => d.PrerequisiteTask != null)
                    .Select(d => d.PrerequisiteTask.StartDate)
                    .DefaultIfEmpty(task.EndDate)
                    .Min();
                latestStart[task.Id] = minLatestStart;
                slack[task.Id] = (latestStart[task.Id] - earliestStart[task.Id]).TotalDays;
            }

            // Critical path consists of tasks with zero slack
            return criticalPath.Where(t => slack[t.Id] == 0).ToList();
        }

        public async Task<List<SchedulingSuggestion>> GetSchedulingSuggestionsAsync(JobTask task)
        {
            var suggestions = new List<SchedulingSuggestion>();
            
            // Check material availability
            var materialConflicts = await DetectConflictsAsync(task);
            if (materialConflicts.Any())
            {
                suggestions.Add(new SchedulingSuggestion
                {
                    Type = SuggestionType.MaterialUnavailable,
                    Priority = SuggestionPriority.High,
                    Message = "Material conflicts detected"
                });
            }

            // Check technician availability
            var technician = await FindEarliestAvailableTechnicianAsync(task);
            if (technician == null)
            {
                suggestions.Add(new SchedulingSuggestion
                {
                    Type = SuggestionType.TechnicianUnavailable,
                    Priority = SuggestionPriority.High,
                    Message = "No available technicians found"
                });
            }

            // Check dependencies
            var dependencies = await _context.TaskDependencies
                .Include(d => d.ParentTask)
                .Include(d => d.PrerequisiteTask)
                .Where(d => d.ParentTaskId == task.Id)
                .ToListAsync();

            foreach (var dependency in dependencies)
            {
                if (dependency.PrerequisiteTask.EndDate > task.StartDate)
                {
                    suggestions.Add(new SchedulingSuggestion
                    {
                        Type = SuggestionType.DependencyConflict,
                        Priority = SuggestionPriority.Medium,
                        Message = $"Dependency conflict with task {dependency.PrerequisiteTask.Name}"
                    });
                }
            }

            return suggestions;
        }

        public async Task<bool> ValidateScheduleAsync(JobTask task)
        {
            // Check material availability
            var materialConflicts = await DetectConflictsAsync(task);
            if (materialConflicts.Any())
                return false;

            // Check technician availability
            var technician = await FindEarliestAvailableTechnicianAsync(task);
            if (technician == null)
                return false;

            // Check dependencies
            var dependencies = await _context.TaskDependencies
                .Include(d => d.ParentTask)
                .Include(d => d.PrerequisiteTask)
                .Where(d => d.ParentTaskId == task.Id)
                .ToListAsync();

            foreach (var dependency in dependencies)
            {
                if (dependency.PrerequisiteTask.EndDate > task.StartDate)
                    return false;
            }

            return true;
        }

        public async Task<DateTime?> FindOptimalStartDateAsync(JobTask task)
        {
            var earliestStart = task.StartDate;
            var latestStart = task.EndDate.AddDays(-1);
            var optimalDate = earliestStart;

            while (optimalDate <= latestStart)
            {
                task.StartDate = optimalDate;
                task.EndDate = optimalDate.AddDays(1);

                if (await ValidateScheduleAsync(task))
                    return optimalDate;

                optimalDate = optimalDate.AddDays(1);
            }

            return null;
        }

        // Lock a schedule slot
        public async Task<bool> LockSlotAsync(ScheduleSlot slot)
        {
            try
            {
                slot.IsLocked = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Unlock a schedule slot
        public async Task<bool> UnlockSlotAsync(ScheduleSlot slot)
        {
            try
            {
                slot.IsLocked = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ShowConflicts(List<ResourceConflict> conflicts)
        {
            foreach (var conflict in conflicts)
            {
                MessageBox.Show($"Conflict detected: {conflict.Type} - {conflict.ResourceId}", "Resource Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
} 