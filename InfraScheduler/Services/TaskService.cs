using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace InfraScheduler.Services
{
    public class TaskService
    {
        private readonly InfraSchedulerContext _context;
        private readonly EquipmentService _equipmentService;

        public TaskService(InfraSchedulerContext context)
        {
            _context = context;
            _equipmentService = new EquipmentService(context);
        }

        public async Task MarkTaskComplete(int taskId)
        {
            var task = await _context.JobTasks
                .Include(t => t.Job)
                .Include(t => t.JobTaskEquipmentLines)
                .ThenInclude(jtel => jtel.EquipmentLine)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                throw new ArgumentException($"Task with ID {taskId} not found");

            // Mark task as complete
            task.Status = "Completed";
            task.CompletedAt = DateTime.UtcNow;

            // Check equipment lines associated with this task
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

            await _context.SaveChangesAsync();
        }
    }
} 