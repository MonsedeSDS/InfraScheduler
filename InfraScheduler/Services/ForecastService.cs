using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfraScheduler.Services
{
    public class ForecastService
    {
        private readonly InfraSchedulerContext _context;

        public ForecastService(InfraSchedulerContext context)
        {
            _context = context;
        }

        public async Task<List<string>> ForecastJobTaskAsync(JobTask task)
        {
            var issues = new List<string>();

            // Check material availability
            var requirements = await _context.MaterialRequirements
                .Where(mr => mr.JobTaskId == task.Id)
                .Include(mr => mr.Material)
                .ToListAsync();

            foreach (var requirement in requirements)
            {
                var material = requirement.Material;
                var totalRequired = await _context.MaterialRequirements
                    .Where(mr => mr.MaterialId == material.Id &&
                                mr.JobTask.StartDate <= task.EndDate &&
                                mr.JobTask.EndDate >= task.StartDate)
                    .SumAsync(mr => mr.Quantity);

                if (totalRequired > material.StockQuantity)
                {
                    issues.Add($"Material '{material.Name}' stock insufficient. Required: {totalRequired}, Available: {material.StockQuantity}");
                }
            }

            return issues;
        }
    }
}
