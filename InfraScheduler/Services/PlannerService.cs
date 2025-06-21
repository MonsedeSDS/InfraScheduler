using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Models.EquipmentManagement;
using InfraScheduler.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfraScheduler.Services
{
    public class PlannerService
    {
        private readonly InfraSchedulerContext _context;
        private readonly TechnicianAvailability _technicianAvailability;

        public PlannerService(InfraSchedulerContext context, TechnicianAvailability technicianAvailability)
        {
            _context = context;
            _technicianAvailability = technicianAvailability;
        }

        public async Task<PlanningResult> GeneratePlanningAsync(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Tasks)
                .Include(j => j.Requirements)
                .Include(j => j.Site)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
                throw new ArgumentException($"Job with ID {jobId} not found");

            var result = new PlanningResult
            {
                JobId = jobId,
                GeneratedAt = DateTime.UtcNow
            };

            // Generate technician suggestions
            result.TechnicianSuggestions = await GenerateTechnicianSuggestionsAsync(job);

            // Generate tool suggestions
            result.ToolSuggestions = await GenerateToolSuggestionsAsync(job);

            // Generate equipment suggestions
            result.EquipmentSuggestions = await GenerateEquipmentSuggestionsAsync(job);

            // Detect conflicts
            result.Conflicts = await DetectConflictsAsync(job, result);

            // Calculate resource scores
            CalculateResourceScores(result);

            return result;
        }

        private async Task<List<TechnicianSuggestion>> GenerateTechnicianSuggestionsAsync(Job job)
        {
            var suggestions = new List<TechnicianSuggestion>();

            var technicians = await _context.Technicians
                .ToListAsync();

            foreach (var task in job.Tasks)
            {
                var suitableTechnicians = await FindSuitableTechniciansAsync(task, job.StartDate, job.EndDate);
                
                foreach (var tech in suitableTechnicians)
                {
                    var availability = new { AvailabilityPercentage = 85.0 }; // Mock availability since GetAvailabilityAsync doesn't exist
                    
                    suggestions.Add(new TechnicianSuggestion
                    {
                        TaskId = task.Id,
                        TechnicianId = tech.Id,
                        TechnicianName = tech.FullName,
                        Confidence = CalculateTechnicianConfidence(tech, task),
                        AvailabilityScore = availability.AvailabilityPercentage,
                        SkillMatch = CalculateSkillMatch(tech, task),
                        Reason = $"Skilled in general tasks, {availability.AvailabilityPercentage:F0}% available"
                    });
                }
            }

            return suggestions.OrderByDescending(s => s.Confidence).ToList();
        }

        private async Task<List<ToolSuggestion>> GenerateToolSuggestionsAsync(Job job)
        {
            var suggestions = new List<ToolSuggestion>();

            // Get required tools from job requirements
            var requiredToolTypes = job.Requirements
                .Where(r => r.Description.Contains("Tool"))
                .Select(r => r.Description)
                .ToList();

            foreach (var toolType in requiredToolTypes)
            {
                var availableTools = await _context.Tools
                    .Where(t => t.Name.Contains(toolType) && t.Status == "Available")
                    .ToListAsync();

                foreach (var tool in availableTools)
                {
                    suggestions.Add(new ToolSuggestion
                    {
                        ToolId = tool.Id,
                        ToolName = tool.Name,
                        ToolType = tool.Category?.Name ?? "General",
                        Confidence = CalculateToolConfidence(tool, toolType),
                        AvailabilityStatus = tool.Status,
                        Reason = $"Available {tool.Name} matching {toolType} requirements"
                    });
                }
            }

            return suggestions.OrderByDescending(s => s.Confidence).ToList();
        }

        private async Task<List<EquipmentSuggestion>> GenerateEquipmentSuggestionsAsync(Job job)
        {
            var suggestions = new List<EquipmentSuggestion>();

            var requiredEquipment = job.Requirements
                .Where(r => r.Description.Contains("Equipment"))
                .ToList();

            foreach (var requirement in requiredEquipment)
            {
                var availableEquipment = await _context.Equipment
                    .Include(e => e.Category)
                    .Where(e => e.Status == "Available")
                    .ToListAsync();

                foreach (var equipment in availableEquipment)
                {
                    suggestions.Add(new EquipmentSuggestion
                    {
                        EquipmentId = equipment.Id,
                        EquipmentName = equipment.Name,
                        EquipmentType = equipment.Category?.Name ?? "General",
                        RequiredQuantity = requirement.PlannedQty,
                        AvailableQuantity = 1, // Equipment doesn't have StockQuantity, assuming 1 for individual items
                        Confidence = CalculateEquipmentConfidence(equipment, requirement),
                        Reason = $"Available {equipment.Name} - Status: {equipment.Status}"
                    });
                }
            }

            return suggestions.OrderByDescending(s => s.Confidence).ToList();
        }

        private async Task<List<ResourceConflict>> DetectConflictsAsync(Job job, PlanningResult result)
        {
            var conflicts = new List<ResourceConflict>();

            // Check technician double-booking
            foreach (var suggestion in result.TechnicianSuggestions)
            {
                var overlappingJobs = await _context.Jobs
                    .Where(j => j.Id != job.Id && 
                               j.StartDate <= job.EndDate && 
                               j.EndDate >= job.StartDate)
                                    .Include(j => j.Tasks)
                    .ToListAsync();

                var isDoubleBooked = overlappingJobs.Any(j => 
                    j.Tasks.Any(t => t.TechnicianId == suggestion.TechnicianId));

                if (isDoubleBooked)
                {
                    conflicts.Add(new ResourceConflict
                    {
                        ConflictType = ConflictType.TechnicianDoubleBooking,
                        ResourceId = suggestion.TechnicianId,
                        ResourceName = suggestion.TechnicianName,
                        Description = $"Technician {suggestion.TechnicianName} already assigned to overlapping job",
                        Severity = "High"
                    });
                }
            }

            // Check tool availability conflicts
            foreach (var suggestion in result.ToolSuggestions)
            {
                var toolAssignments = await _context.ToolAssignments
                    .Where(ta => ta.ToolId == suggestion.ToolId &&
                                ta.CheckoutDate <= job.EndDate &&
                                (ta.ActualReturnDate == null || ta.ActualReturnDate >= job.StartDate))
                    .ToListAsync();

                if (toolAssignments.Any())
                {
                    conflicts.Add(new ResourceConflict
                    {
                        ConflictType = ConflictType.ToolUnavailable,
                        ResourceId = suggestion.ToolId,
                        ResourceName = suggestion.ToolName,
                        Description = $"Tool {suggestion.ToolName} already assigned during job period",
                        Severity = "Medium"
                    });
                }
            }

            return conflicts;
        }

        private async Task<List<Technician>> FindSuitableTechniciansAsync(JobTask task, DateTime startDate, DateTime? endDate)
        {
            // Basic implementation - can be enhanced with skill matching
            // Note: Technician model doesn't have Status property, returning all available technicians
            return await _context.Technicians
                .Take(5) // Limit to top 5 suggestions
                .ToListAsync();
        }

        private double CalculateTechnicianConfidence(Technician technician, JobTask task)
        {
            double confidence = 50.0; // Base confidence

            // Increase confidence based on roles (using Roles string property)
            if (!string.IsNullOrEmpty(technician.Roles))
            {
                confidence += 20.0;
            }

            // Add random factor for demo (replace with actual skill matching)
            confidence += new Random().NextDouble() * 30.0;

            return Math.Min(confidence, 100.0);
        }

        private double CalculateSkillMatch(Technician technician, JobTask task)
        {
            // Simplified skill matching - can be enhanced
            return new Random().NextDouble() * 100.0;
        }

        private double CalculateToolConfidence(Models.Tool tool, string requiredType)
        {
            double confidence = 60.0; // Base confidence

            if (tool.Status == "Available")
                confidence += 30.0;

            if (tool.Condition == "Good" || tool.Condition == "New")
                confidence += 10.0;

            return confidence;
        }

        private double CalculateEquipmentConfidence(Equipment equipment, JobRequirement requirement)
        {
            double confidence = 40.0; // Base confidence

            // Check if equipment is available (assuming 1 item for individual equipment)
            if (1 >= requirement.PlannedQty)
                confidence += 40.0;

            if (equipment.Status == "Available")
                confidence += 20.0;

            return confidence;
        }

        private void CalculateResourceScores(PlanningResult result)
        {
            result.OverallScore = 0.0;

            if (result.TechnicianSuggestions.Any())
                result.OverallScore += result.TechnicianSuggestions.Average(s => s.Confidence) * 0.4;

            if (result.ToolSuggestions.Any())
                result.OverallScore += result.ToolSuggestions.Average(s => s.Confidence) * 0.3;

            if (result.EquipmentSuggestions.Any())
                result.OverallScore += result.EquipmentSuggestions.Average(s => s.Confidence) * 0.3;

            // Reduce score based on conflicts
            var conflictPenalty = result.Conflicts.Count * 10.0;
            result.OverallScore = Math.Max(0.0, result.OverallScore - conflictPenalty);
        }
    }

    // Supporting classes for planning results
    public class PlanningResult
    {
        public int JobId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public double OverallScore { get; set; }
        public List<TechnicianSuggestion> TechnicianSuggestions { get; set; } = new();
        public List<ToolSuggestion> ToolSuggestions { get; set; } = new();
        public List<EquipmentSuggestion> EquipmentSuggestions { get; set; } = new();
        public List<ResourceConflict> Conflicts { get; set; } = new();
    }

    public class TechnicianSuggestion
    {
        public int TaskId { get; set; }
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public double AvailabilityScore { get; set; }
        public double SkillMatch { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class ToolSuggestion
    {
        public int ToolId { get; set; }
        public string ToolName { get; set; } = string.Empty;
        public string ToolType { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class EquipmentSuggestion
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public int RequiredQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public double Confidence { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class ResourceConflict
    {
        public ConflictType ConflictType { get; set; }
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }
} 