using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace InfraScheduler.Services
{
    public class MaterialRequirementService
    {
        private readonly InfraSchedulerContext _context;

        public MaterialRequirementService(InfraSchedulerContext context)
        {
            _context = context;
        }

        public async Task<List<MaterialRequirement>> GetMaterialRequirementsForTask(int jobTaskId)
        {
            return await _context.MaterialRequirements
                .Include(mr => mr.Material)
                .Where(mr => mr.JobTaskId == jobTaskId)
                .ToListAsync();
        }

        public async Task<bool> AddMaterialRequirement(MaterialRequirement requirement)
        {
            try
            {
                _context.MaterialRequirements.Add(requirement);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveMaterialRequirement(int requirementId)
        {
            try
            {
                var requirement = await _context.MaterialRequirements.FindAsync(requirementId);
                if (requirement != null)
                {
                    _context.MaterialRequirements.Remove(requirement);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
} 