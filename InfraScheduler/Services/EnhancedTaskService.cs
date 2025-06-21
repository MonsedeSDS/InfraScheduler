using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfraScheduler.Services
{
    public class EnhancedTaskService
    {
        private readonly InfraSchedulerContext _context;
        private readonly PlannerService _plannerService;

        public EnhancedTaskService(InfraSchedulerContext context, PlannerService plannerService)
        {
            _context = context;
            _plannerService = plannerService;
        }

        public async Task<TaskSchedulingResult> AutoScheduleTasksAsync(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Tasks)
                    .ThenInclude(t => t.Dependencies)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
                throw new ArgumentException($"Job with ID {jobId} not found");

            var result = new TaskSchedulingResult
            {
                JobId = jobId,
                ScheduledAt = DateTime.UtcNow
            };

            try
            {
                // Build dependency graph
                var dependencyGraph = BuildDependencyGraph(job.Tasks.ToList());
                
                // Calculate critical path
                var criticalPath = CalculateCriticalPath(dependencyGraph);
                result.CriticalPath = criticalPath;

                // Schedule tasks respecting dependencies
                var scheduledTasks = await ScheduleTasksWithDependencies(job.Tasks.ToList(), job.StartDate);
                result.ScheduledTasks = scheduledTasks;

                // Detect scheduling conflicts
                result.Conflicts = DetectSchedulingConflicts(scheduledTasks);

                // Calculate total duration
                result.TotalDuration = CalculateTotalDuration(scheduledTasks);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public async Task<BatchOperationResult> UpdateTaskStatusBatchAsync(List<int> taskIds, string newStatus)
        {
            var result = new BatchOperationResult
            {
                TotalItems = taskIds.Count,
                ProcessedAt = DateTime.UtcNow
            };

            try
            {
                var tasks = await _context.JobTasks
                    .Where(t => taskIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var task in tasks)
                {
                    var oldStatus = task.Status;
                    task.Status = newStatus;

                    // Update progress based on status
                    task.Progress = newStatus switch
                    {
                        "Not Started" => 0.0,
                        "In Progress" => 50.0,
                        "Completed" => 100.0,
                        "On Hold" => task.Progress,
                        _ => task.Progress
                    };

                    result.SuccessfulItems.Add(new BatchItem
                    {
                        Id = task.Id,
                        Name = task.Name,
                        OldValue = oldStatus,
                        NewValue = newStatus
                    });
                }

                await _context.SaveChangesAsync();
                result.Success = true;
                result.SuccessCount = result.SuccessfulItems.Count;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public async Task<ConflictResolutionResult> ResolveResourceConflictsAsync(int jobId)
        {
            var planningResult = await _plannerService.GeneratePlanningAsync(jobId);
            
            var result = new ConflictResolutionResult
            {
                JobId = jobId,
                ResolvedAt = DateTime.UtcNow,
                TotalConflicts = planningResult.Conflicts.Count
            };

            foreach (var conflict in planningResult.Conflicts)
            {
                var resolution = await ResolveConflictAsync(conflict);
                result.Resolutions.Add(resolution);
            }

            result.Success = result.Resolutions.All(r => r.Success);
            
            return result;
        }

        public async Task<List<TaskProgressUpdate>> CalculateProgressUpdatesAsync(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Tasks)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null) return new List<TaskProgressUpdate>();

            var updates = new List<TaskProgressUpdate>();

            foreach (var task in job.Tasks)
            {
                var progressUpdate = new TaskProgressUpdate
                {
                    TaskId = task.Id,
                    TaskName = task.Name,
                    OldProgress = task.Progress,
                    NewProgress = CalculateTaskProgress(task),
                    UpdatedAt = DateTime.UtcNow
                };

                if (Math.Abs(progressUpdate.NewProgress - progressUpdate.OldProgress) > 0.01)
                {
                    updates.Add(progressUpdate);
                }
            }

            return updates;
        }

        private Dictionary<int, List<int>> BuildDependencyGraph(List<JobTask> tasks)
        {
            var graph = new Dictionary<int, List<int>>();

            foreach (var task in tasks)
            {
                graph[task.Id] = task.Dependencies.Select(d => d.PrerequisiteTaskId).ToList();
            }

            return graph;
        }

        private List<int> CalculateCriticalPath(Dictionary<int, List<int>> dependencyGraph)
        {
            // Simplified critical path calculation
            // In a real implementation, this would use proper CPM algorithm
            var criticalPath = new List<int>();
            
            var visited = new HashSet<int>();
            var longestPath = new Dictionary<int, int>();

            foreach (var taskId in dependencyGraph.Keys)
            {
                if (!visited.Contains(taskId))
                {
                    CalculateLongestPath(taskId, dependencyGraph, visited, longestPath);
                }
            }

            // Return tasks in order of longest path
            criticalPath = longestPath
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            return criticalPath;
        }

        private int CalculateLongestPath(int taskId, Dictionary<int, List<int>> graph, HashSet<int> visited, Dictionary<int, int> longestPath)
        {
            if (visited.Contains(taskId))
                return longestPath.GetValueOrDefault(taskId, 0);

            visited.Add(taskId);

            int maxDependencyPath = 0;
            if (graph.ContainsKey(taskId))
            {
                foreach (var dependency in graph[taskId])
                {
                    int depPath = CalculateLongestPath(dependency, graph, visited, longestPath);
                    maxDependencyPath = Math.Max(maxDependencyPath, depPath);
                }
            }

            longestPath[taskId] = maxDependencyPath + 1;
            return longestPath[taskId];
        }

        private async Task<List<ScheduledTask>> ScheduleTasksWithDependencies(List<JobTask> tasks, DateTime projectStartDate)
        {
            var scheduledTasks = new List<ScheduledTask>();
            var taskStartDates = new Dictionary<int, DateTime>();

            // Sort tasks by dependency order
            var sortedTasks = TopologicalSort(tasks);

            foreach (var task in sortedTasks)
            {
                var earliestStart = projectStartDate;

                // Check dependencies
                foreach (var dependency in task.Dependencies)
                {
                    if (taskStartDates.ContainsKey(dependency.PrerequisiteTaskId))
                    {
                        var dependentEndDate = taskStartDates[dependency.PrerequisiteTaskId].AddDays(task.Duration);
                        earliestStart = earliestStart > dependentEndDate ? earliestStart : dependentEndDate;
                    }
                }

                taskStartDates[task.Id] = earliestStart;

                scheduledTasks.Add(new ScheduledTask
                {
                    TaskId = task.Id,
                    TaskName = task.Name,
                    ScheduledStartDate = earliestStart,
                    ScheduledEndDate = earliestStart.AddDays(task.Duration),
                    EstimatedDuration = task.Duration
                });
            }

            return scheduledTasks;
        }

        private List<JobTask> TopologicalSort(List<JobTask> tasks)
        {
            // Simplified topological sort
            var visited = new HashSet<int>();
            var sorted = new List<JobTask>();

            foreach (var task in tasks)
            {
                if (!visited.Contains(task.Id))
                {
                    TopologicalSortUtil(task, tasks, visited, sorted);
                }
            }

            return sorted;
        }

        private void TopologicalSortUtil(JobTask task, List<JobTask> allTasks, HashSet<int> visited, List<JobTask> sorted)
        {
            visited.Add(task.Id);

            // Visit dependencies first
            foreach (var dependency in task.Dependencies)
            {
                var dependentTask = allTasks.FirstOrDefault(t => t.Id == dependency.PrerequisiteTaskId);
                if (dependentTask != null && !visited.Contains(dependentTask.Id))
                {
                    TopologicalSortUtil(dependentTask, allTasks, visited, sorted);
                }
            }

            sorted.Add(task);
        }

        private List<SchedulingConflict> DetectSchedulingConflicts(List<ScheduledTask> scheduledTasks)
        {
            var conflicts = new List<SchedulingConflict>();

            // Check for overlapping resource assignments
            var resourceAssignments = new Dictionary<int, List<ScheduledTask>>();

            foreach (var task in scheduledTasks)
            {
                // This would be enhanced to check actual resource assignments
                // For now, just check for unrealistic scheduling
                if (task.ScheduledEndDate < task.ScheduledStartDate)
                {
                    conflicts.Add(new SchedulingConflict
                    {
                        TaskId = task.TaskId,
                        TaskName = task.TaskName,
                        ConflictType = "Invalid Date Range",
                        Description = "End date is before start date"
                    });
                }
            }

            return conflicts;
        }

        private double CalculateTotalDuration(List<ScheduledTask> scheduledTasks)
        {
            if (!scheduledTasks.Any()) return 0;

            var earliestStart = scheduledTasks.Min(t => t.ScheduledStartDate);
            var latestEnd = scheduledTasks.Max(t => t.ScheduledEndDate);

            return (latestEnd - earliestStart).TotalDays;
        }

        private async Task<ConflictResolution> ResolveConflictAsync(ResourceConflict conflict)
        {
            var resolution = new ConflictResolution
            {
                ConflictId = conflict.ResourceId,
                ConflictType = conflict.ConflictType.ToString(),
                AttemptedAt = DateTime.UtcNow
            };

            try
            {
                switch (conflict.ConflictType)
                {
                    case ConflictType.TechnicianDoubleBooking:
                        resolution = await ResolveTechnicianConflictAsync(conflict);
                        break;
                    case ConflictType.ToolUnavailable:
                        resolution = await ResolveToolConflictAsync(conflict);
                        break;
                    default:
                        resolution.Success = false;
                        resolution.Resolution = "No resolution strategy available";
                        break;
                }
            }
            catch (Exception ex)
            {
                resolution.Success = false;
                resolution.Resolution = $"Error resolving conflict: {ex.Message}";
            }

            return resolution;
        }

        private async Task<ConflictResolution> ResolveTechnicianConflictAsync(ResourceConflict conflict)
        {
            // Find alternative technicians
            var alternativeTechnicians = await _context.Technicians
                .Where(t => t.Id != conflict.ResourceId)
                .Take(3)
                .ToListAsync();

            return new ConflictResolution
            {
                ConflictId = conflict.ResourceId,
                ConflictType = conflict.ConflictType.ToString(),
                Success = alternativeTechnicians.Any(),
                Resolution = alternativeTechnicians.Any() 
                    ? $"Suggested alternatives: {string.Join(", ", alternativeTechnicians.Select(t => t.FullName))}"
                    : "No alternative technicians available",
                AttemptedAt = DateTime.UtcNow
            };
        }

        private async Task<ConflictResolution> ResolveToolConflictAsync(ResourceConflict conflict)
        {
            // Find alternative tools
            var tool = await _context.Tools.FindAsync(conflict.ResourceId);
            if (tool == null)
            {
                return new ConflictResolution
                {
                    ConflictId = conflict.ResourceId,
                    Success = false,
                    Resolution = "Tool not found"
                };
            }

            var alternativeTools = await _context.Tools
                .Where(t => t.Id != conflict.ResourceId && 
                           t.Name.Contains(tool.Name) && 
                           t.Status == "Available")
                .Take(3)
                .ToListAsync();

            return new ConflictResolution
            {
                ConflictId = conflict.ResourceId,
                ConflictType = conflict.ConflictType.ToString(),
                Success = alternativeTools.Any(),
                Resolution = alternativeTools.Any()
                    ? $"Suggested alternatives: {string.Join(", ", alternativeTools.Select(t => t.Name))}"
                    : "No alternative tools available",
                AttemptedAt = DateTime.UtcNow
            };
        }

        private double CalculateTaskProgress(JobTask task)
        {
            // Enhanced progress calculation based on multiple factors
            double progress = task.Progress;

            // Factor in time elapsed
            if (task.StartDate <= DateTime.Now)
            {
                var totalDuration = (task.EndDate - task.StartDate).TotalDays;
                var elapsedDuration = (DateTime.Now - task.StartDate).TotalDays;
                var timeBasedProgress = Math.Min(100.0, (elapsedDuration / totalDuration) * 100.0);

                // Weighted average of current progress and time-based progress
                progress = (progress * 0.7) + (timeBasedProgress * 0.3);
            }

            return Math.Min(100.0, Math.Max(0.0, progress));
        }
    }

    // Supporting classes
    public class TaskSchedulingResult
    {
        public int JobId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<int> CriticalPath { get; set; } = new();
        public List<ScheduledTask> ScheduledTasks { get; set; } = new();
        public List<SchedulingConflict> Conflicts { get; set; } = new();
        public double TotalDuration { get; set; }
    }

    public class ScheduledTask
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public DateTime ScheduledStartDate { get; set; }
        public DateTime ScheduledEndDate { get; set; }
        public double EstimatedDuration { get; set; }
    }

    public class SchedulingConflict
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string ConflictType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class BatchOperationResult
    {
        public int TotalItems { get; set; }
        public int SuccessCount { get; set; }
        public DateTime ProcessedAt { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<BatchItem> SuccessfulItems { get; set; } = new();
        public List<BatchItem> FailedItems { get; set; } = new();
    }

    public class BatchItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class ConflictResolutionResult
    {
        public int JobId { get; set; }
        public DateTime ResolvedAt { get; set; }
        public int TotalConflicts { get; set; }
        public bool Success { get; set; }
        public List<ConflictResolution> Resolutions { get; set; } = new();
    }

    public class ConflictResolution
    {
        public int ConflictId { get; set; }
        public string ConflictType { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public DateTime AttemptedAt { get; set; }
    }

    public class TaskProgressUpdate
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public double OldProgress { get; set; }
        public double NewProgress { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 