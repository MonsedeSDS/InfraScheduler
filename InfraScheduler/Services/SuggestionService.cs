using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using InfraScheduler.Services.Interfaces;

namespace InfraScheduler.Services
{
    public class SuggestionService
    {
        private readonly InfraSchedulerContext _context;
        private readonly IMaterialAvailability _materialAvailability;
        private readonly ITechnicianAvailability _technicianAvailability;

        public SuggestionService(
            InfraSchedulerContext context,
            IMaterialAvailability materialAvailability,
            ITechnicianAvailability technicianAvailability)
        {
            _context = context;
            _materialAvailability = materialAvailability;
            _technicianAvailability = technicianAvailability;
        }

        public async Task<List<SchedulingSuggestion>> GetSchedulingSuggestionsAsync(JobTask task)
        {
            var suggestions = new List<SchedulingSuggestion>();

            // Get material requirements for the task
            var materialRequirements = await _context.MaterialRequirements
                .Where(mr => mr.JobTaskId == task.Id)
                .ToListAsync();

            // Check material availability
            var materialAvailability = materialRequirements
                .Select(mr => _materialAvailability.GetMaterialAvailability(
                    mr.MaterialId,
                    task.StartDate,
                    task.EndDate))
                .SelectMany(x => x)
                .ToList();

            if (!materialAvailability.Any())
            {
                suggestions.Add(new SchedulingSuggestion
                {
                    Type = SuggestionType.MaterialUnavailable,
                    Message = "Required materials are not available for the scheduled period",
                    Priority = SuggestionPriority.High
                });
            }

            // Check technician availability
            if (task.AssignedTechnicianId.HasValue)
            {
                var technicianAvailability = _technicianAvailability.GetTechnicianAvailability(
                    task.AssignedTechnicianId.Value,
                    task.StartDate,
                    task.EndDate);

                if (!technicianAvailability.Any())
                {
                    suggestions.Add(new SchedulingSuggestion
                    {
                        Type = SuggestionType.TechnicianUnavailable,
                        Message = "Assigned technician is not available for the scheduled period",
                        Priority = SuggestionPriority.High
                    });
                }
            }

            // Check task dependencies
            var dependencies = await _context.TaskDependencies
                .Include(d => d.ParentTask)
                .Where(d => d.PrerequisiteTaskId == task.Id)
                .ToListAsync();

            foreach (var dependency in dependencies)
            {
                if (dependency.ParentTask.EndDate > task.StartDate)
                {
                    suggestions.Add(new SchedulingSuggestion
                    {
                        Type = SuggestionType.DependencyConflict,
                        Message = $"Task depends on '{dependency.ParentTask.Name}' which ends after this task's start date",
                        Priority = SuggestionPriority.High
                    });
                }
            }

            // Check resource calendar conflicts
            var resourceConflicts = await _context.ResourceCalendars
                .Include(rc => rc.Technician)
                .Where(rc => rc.TechnicianId == task.AssignedTechnicianId &&
                            rc.Date >= task.StartDate &&
                            rc.Date <= task.EndDate)
                .ToListAsync();

            foreach (var conflict in resourceConflicts)
            {
                suggestions.Add(new SchedulingSuggestion
                {
                    Type = SuggestionType.ResourceConflict,
                    Message = $"Technician has a calendar entry on {conflict.Date:yyyy-MM-dd}",
                    Priority = SuggestionPriority.Medium
                });
            }

            // Check for optimal scheduling windows
            var optimalWindows = await FindOptimalSchedulingWindowsAsync(task);
            foreach (var window in optimalWindows)
            {
                suggestions.Add(new SchedulingSuggestion
                {
                    Type = SuggestionType.OptimalWindow,
                    Message = $"Optimal scheduling window: {window.Start:yyyy-MM-dd} to {window.End:yyyy-MM-dd}",
                    Priority = SuggestionPriority.Low
                });
            }

            return suggestions;
        }

        private async Task<List<TimeWindow>> FindOptimalSchedulingWindowsAsync(JobTask task)
        {
            var windows = new List<TimeWindow>();
            var currentDate = DateTime.Now;

            // Get material requirements for the task
            var materialRequirements = await _context.MaterialRequirements
                .Where(mr => mr.JobTaskId == task.Id)
                .ToListAsync();

            // Look for windows in the next 30 days
            for (int i = 0; i < 30; i++)
            {
                var checkDate = currentDate.AddDays(i);
                var endDate = checkDate.AddDays(task.Duration);

                // Check if materials are available
                var materialAvailability = materialRequirements
                    .Select(mr => _materialAvailability.GetMaterialAvailability(
                        mr.MaterialId,
                        checkDate,
                        endDate))
                    .SelectMany(x => x)
                    .ToList();

                if (!materialAvailability.Any()) continue;

                // Check if technician is available
                if (task.AssignedTechnicianId.HasValue)
                {
                    var technicianAvailability = _technicianAvailability.GetTechnicianAvailability(
                        task.AssignedTechnicianId.Value,
                        checkDate,
                        endDate);

                    if (!technicianAvailability.Any()) continue;
                }

                // Check for resource calendar conflicts
                var hasConflict = await _context.ResourceCalendars
                    .AnyAsync(rc => rc.TechnicianId == task.AssignedTechnicianId &&
                                  rc.Date >= checkDate &&
                                  rc.Date <= endDate);

                if (!hasConflict)
                {
                    windows.Add(new TimeWindow
                    {
                        Start = checkDate,
                        End = endDate
                    });
                }
            }

            return windows;
        }

        public async Task<DateTime?> SuggestTaskSchedule(JobTask task)
        {
            if (task == null)
                return null;

            // Get all dependencies
            var dependencies = await _context.TaskDependencies
                .Where(d => d.PrerequisiteTaskId == task.Id)
                .Include(d => d.ParentTask)
                .ToListAsync();

            // Find the latest end date among dependencies
            var latestDependencyEnd = dependencies
                .Select(d => d.ParentTask.EndDate)
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();

            // Get material requirements for the task
            var materialRequirements = await _context.MaterialRequirements
                .Where(mr => mr.JobTaskId == task.Id)
                .ToListAsync();

            // Check material availability
            var materialAvailability = materialRequirements
                .Select(mr => _materialAvailability.GetMaterialAvailability(
                    mr.MaterialId,
                    latestDependencyEnd,
                    latestDependencyEnd.AddDays(task.Duration)))
                .SelectMany(x => x)
                .ToList();

            if (!materialAvailability.Any())
                return null;

            // Check technician availability
            if (task.AssignedTechnicianId.HasValue)
            {
                var technicianAvailability = _technicianAvailability.GetTechnicianAvailability(
                    task.AssignedTechnicianId.Value,
                    latestDependencyEnd,
                    latestDependencyEnd.AddDays(task.Duration));

                if (!technicianAvailability.Any())
                    return null;
            }

            // Return the suggested start date
            return latestDependencyEnd > DateTime.MinValue ? latestDependencyEnd : DateTime.Now;
        }
    }

    public class TimeWindow
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class SchedulingSuggestion
    {
        public SuggestionType Type { get; set; }
        public string Message { get; set; }
        public SuggestionPriority Priority { get; set; }
    }

    public enum SuggestionType
    {
        MaterialUnavailable,
        TechnicianUnavailable,
        DependencyConflict,
        ResourceConflict,
        OptimalWindow
    }

    public enum SuggestionPriority
    {
        Low,
        Medium,
        High
    }
}