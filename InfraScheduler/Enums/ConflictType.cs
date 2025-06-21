namespace InfraScheduler.Enums
{
    public enum ConflictType
    {
        Unknown = 0,
        TechnicianDoubleBooking = 1,
        ToolUnavailable = 2,
        EquipmentShortage = 3,
        TimeConstraint = 4,
        SkillMismatch = 5,
        SiteAccess = 6,
        // Legacy values for compatibility
        Tool = ToolUnavailable,
        Equipment = EquipmentShortage,
        Material = EquipmentShortage,
        Technician = TechnicianDoubleBooking,
        Schedule = TimeConstraint
    }
} 