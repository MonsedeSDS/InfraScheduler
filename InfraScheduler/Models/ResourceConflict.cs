using System;

namespace InfraScheduler.Models
{
    public class ResourceConflict
    {
        public ConflictType Type { get; set; }
        public int ResourceId { get; set; }
        public int ConflictingTaskId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Description => 
            $"Conflict: {Type} (ResourceId: {ResourceId}) with Task {ConflictingTaskId} from {StartDate:d} to {EndDate:d}";
    }

    public enum ConflictType
    {
        Technician,
        Material
    }
} 