using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InfraScheduler.Services
{
    public class MaterialForecastService
    {
        private readonly InfraSchedulerContext _context;

        public MaterialForecastService(InfraSchedulerContext context)
        {
            _context = context;
        }

        public List<string> ForecastMaterialForJob(JobTask jobTask)
        {
            var issues = new List<string>();

            var requirements = _context.MaterialRequirements
                .Include(mr => mr.Material)
                .Include(mr => mr.JobTask)
                .Where(mr => mr.JobTask.PlannedStart <= jobTask.PlannedStart)
                .ToList();

            var groupedRequirements = requirements
                .GroupBy(r => r.MaterialId)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

            foreach (var material in _context.Materials)
            {
                var required = groupedRequirements.ContainsKey(material.Id) ? groupedRequirements[material.Id] : 0;
                var availableAfterRequirement = material.StockQuantity - (int)required;

                if (availableAfterRequirement < 0)
                {
                    issues.Add($"Material '{material.Name}' stock negative ({availableAfterRequirement}) before task start.");
                }
            }

            return issues;
        }

        public MaterialForecast GetMaterialForecast(int materialId, DateTime startDate, DateTime endDate)
        {
            var material = _context.Materials.Find(materialId);
            if (material == null)
            {
                return new MaterialForecast { AvailableQuantity = 0 };
            }

            var requirements = _context.MaterialRequirements
                .Where(r => r.MaterialId == materialId &&
                           r.JobTask.PlannedStart <= endDate &&
                           r.JobTask.PlannedEnd >= startDate)
                .Sum(r => r.Quantity);

            return new MaterialForecast
            {
                AvailableQuantity = material.StockQuantity - (int)requirements
            };
        }

        public DateTime FindEarliestAvailabilityDate(int materialId, int requiredQuantity)
        {
            var material = _context.Materials.Find(materialId);
            if (material == null || material.StockQuantity >= requiredQuantity)
            {
                return DateTime.Now;
            }

            var currentDate = DateTime.Now;
            var maxAttempts = 30; // Look ahead up to 30 days
            var attempts = 0;

            while (attempts < maxAttempts)
            {
                var requirements = _context.MaterialRequirements
                    .Where(r => r.MaterialId == materialId &&
                               r.JobTask.PlannedStart <= currentDate &&
                               r.JobTask.PlannedEnd >= currentDate)
                    .Sum(r => r.Quantity);

                if (material.StockQuantity - (int)requirements >= requiredQuantity)
                {
                    return currentDate;
                }

                currentDate = currentDate.AddDays(1);
                attempts++;
            }

            return currentDate; // Return the last checked date if no availability found
        }
    }

    public class MaterialForecast
    {
        public int AvailableQuantity { get; set; }
    }
}
