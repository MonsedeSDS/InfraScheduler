using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfraScheduler.Services
{
    public class ConflictService
    {
        private readonly InfraSchedulerContext _context;

        public ConflictService(InfraSchedulerContext context)
        {
            _context = context;
        }

        public async Task<List<ResourceConflict>> CheckConflictsAsync(JobTask task)
        {
            var conflicts = new List<ResourceConflict>();

            // Check technician availability
            if (task.AssignedTechnicianId.HasValue)
            {
                var technicianConflicts = await _context.JobTasks
                    .Where(t => t.AssignedTechnicianId == task.AssignedTechnicianId &&
                               t.Id != task.Id &&
                               ((t.StartDate <= task.EndDate && t.EndDate >= task.StartDate)))
                    .ToListAsync();

                foreach (var conflict in technicianConflicts)
                {
                    conflicts.Add(new ResourceConflict
                    {
                        Type = ConflictType.Technician,
                        ResourceId = task.AssignedTechnicianId.Value,
                        ConflictingTaskId = conflict.Id,
                        StartDate = conflict.StartDate,
                        EndDate = conflict.EndDate
                    });
                }
            }

            // Check material availability
            var materialRequirements = await _context.MaterialRequirements
                .Where(mr => mr.JobTaskId == task.Id)
                .Include(mr => mr.Material)
                .ToListAsync();

            foreach (var requirement in materialRequirements)
            {
                var material = requirement.Material;
                var totalRequired = await _context.MaterialRequirements
                    .Where(mr => mr.MaterialId == material.Id &&
                                mr.JobTask.StartDate <= task.EndDate &&
                                mr.JobTask.EndDate >= task.StartDate)
                    .SumAsync(mr => mr.Quantity);

                if (totalRequired > material.StockQuantity)
                {
                    conflicts.Add(new ResourceConflict
                    {
                        Type = ConflictType.Material,
                        ResourceId = material.Id,
                        ConflictingTaskId = task.Id,
                        StartDate = task.StartDate,
                        EndDate = task.EndDate
                    });
                }
            }

            return conflicts;
        }
    }
}
