using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InfraScheduler.Services
{
    public class MaterialAutoReservationService
    {
        private readonly InfraSchedulerContext _context;

        public MaterialAutoReservationService(InfraSchedulerContext context)
        {
            _context = context;
        }

        public List<string> ReserveMaterialsForJob(JobTask task)
        {
            var report = new List<string>();

            // Load required materials for the task
            var requiredMaterials = _context.MaterialRequirements
                .Where(mr => mr.JobTaskId == task.Id)
                .Include(mr => mr.Material)
                .ToList();

            foreach (var req in requiredMaterials)
            {
                var material = req.Material;
                double alreadyRequired = _context.MaterialRequirements
                    .Where(r => r.MaterialId == material.Id)
                    .Sum(r => r.Quantity);

                int availableStock = material.StockQuantity - (int)alreadyRequired;

                if (availableStock >= req.Quantity)
                {
                    report.Add($"✅ Required {req.Quantity} of {material.Name}.");
                }
                else
                {
                    report.Add($"❌ Not enough stock for {material.Name}. Required: {req.Quantity}, Available: {availableStock}");
                }
            }

            return report;
        }
    }
}
