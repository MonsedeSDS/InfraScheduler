using InfraScheduler.Models;
using InfraScheduler.Models.EquipmentManagement;
using Microsoft.EntityFrameworkCore;

namespace InfraScheduler.Data
{
    public class InfraSchedulerContext : DbContext
    {
        public InfraSchedulerContext(DbContextOptions<InfraSchedulerContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Subcontractor> Subcontractors { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<SiteOwner> SiteOwners { get; set; }
        public DbSet<SiteTenant> SiteTenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<JobTask> JobTasks { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<Tool> Tools { get; set; }
        public DbSet<ToolCategory> ToolCategories { get; set; }
        public DbSet<ToolAssignment> ToolAssignments { get; set; }
        public DbSet<ToolMaintenance> ToolMaintenances { get; set; }
        
        // Equipment Management DbSets
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<EquipmentCategory> EquipmentCategories { get; set; }
        public DbSet<EquipmentBatch> EquipmentBatches { get; set; }
        public DbSet<EquipmentLine> EquipmentLines { get; set; }
        public DbSet<EquipmentDiscrepancy> EquipmentDiscrepancies { get; set; }
        public DbSet<EquipmentInventoryRecord> EquipmentInventoryRecords { get; set; }
        public DbSet<SiteEquipmentLedger> SiteEquipmentLedgers { get; set; }
        public DbSet<SiteEquipmentSnapshot> SiteEquipmentSnapshots { get; set; }
        
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<InstallationLog> InstallationLogs { get; set; }
        public DbSet<JobTaskTechnician> JobTaskTechnicians { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<MaterialRequirement> MaterialRequirements => Set<MaterialRequirement>();
        public DbSet<TechnicianAssignment> TechnicianAssignments => Set<TechnicianAssignment>();
        public DbSet<ToolMaintenance> ToolMaintenance => Set<ToolMaintenance>();
        public DbSet<JobTaskEquipmentLine> JobTaskEquipmentLines { get; set; }
        public DbSet<JobRequirement> JobRequirements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many-to-many Site <-> Client via SiteTenant
            modelBuilder.Entity<SiteTenant>()
                .HasKey(st => new { st.SiteId, st.ClientId });

            modelBuilder.Entity<SiteTenant>()
                .HasOne(st => st.Site)
                .WithMany(s => s.SiteTenants)
                .HasForeignKey(st => st.SiteId);

            modelBuilder.Entity<SiteTenant>()
                .HasOne(st => st.Client)
                .WithMany(c => c.SiteTenants)
                .HasForeignKey(st => st.ClientId);

            // Seed SiteOwners data
            modelBuilder.Entity<SiteOwner>().HasData(
                new SiteOwner 
                { 
                    Id = 1, 
                    CompanyName = "Prime Infrastructure & Engineering", 
                    ContactPerson = "John Doe",
                    Phone = "+229 12345678",
                    Email = "contact@primeinfra.bj",
                    Address = "Cotonou, Benin"
                },
                new SiteOwner 
                { 
                    Id = 2, 
                    CompanyName = "Benin Telecom", 
                    ContactPerson = "Jane Smith",
                    Phone = "+229 87654321",
                    Email = "contact@benintelecom.bj",
                    Address = "Porto-Novo, Benin"
                }
            );

            // Configure Tool relationships
            modelBuilder.Entity<Tool>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tools)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tool>()
                .HasOne(t => t.AssignedToJob)
                .WithMany()
                .HasForeignKey(t => t.AssignedToJobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tool>()
                .HasOne(t => t.AssignedToTechnician)
                .WithMany()
                .HasForeignKey(t => t.AssignedToTechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ToolAssignment relationships
            modelBuilder.Entity<ToolAssignment>()
                .HasOne(a => a.Tool)
                .WithMany(t => t.Assignments)
                .HasForeignKey(a => a.ToolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ToolAssignment>()
                .HasOne(a => a.Technician)
                .WithMany()
                .HasForeignKey(a => a.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ToolAssignment>()
                .HasOne(a => a.Job)
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ToolMaintenance relationships
            modelBuilder.Entity<ToolMaintenance>()
                .HasOne(m => m.Tool)
                .WithMany(t => t.MaintenanceHistory)
                .HasForeignKey(m => m.ToolId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Equipment relationships
            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Equipment)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.AssignedToJob)
                .WithMany()
                .HasForeignKey(e => e.AssignedToJobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.AssignedToTechnician)
                .WithMany()
                .HasForeignKey(e => e.AssignedToTechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure EquipmentBatch relationships
            modelBuilder.Entity<EquipmentBatch>()
                .HasMany(b => b.Lines)
                .WithOne(l => l.Batch)
                .HasForeignKey(l => l.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure EquipmentLine relationships
            modelBuilder.Entity<EquipmentLine>()
                .HasMany(l => l.Discrepancies)
                .WithOne(d => d.Line)
                .HasForeignKey(d => d.LineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EquipmentLine>()
                .HasOne(l => l.EquipmentType)
                .WithMany(e => e.EquipmentLines)
                .HasForeignKey(l => l.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure EquipmentInventoryRecord relationships
            modelBuilder.Entity<EquipmentInventoryRecord>()
                .HasOne(r => r.EquipmentType)
                .WithMany(e => e.InventoryRecords)
                .HasForeignKey(r => r.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EquipmentInventoryRecord>()
                .HasOne(r => r.Site)
                .WithMany()
                .HasForeignKey(r => r.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure SiteEquipmentLedger relationships
            modelBuilder.Entity<SiteEquipmentLedger>()
                .HasOne(l => l.Site)
                .WithMany()
                .HasForeignKey(l => l.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SiteEquipmentLedger>()
                .HasOne(l => l.EquipmentType)
                .WithMany(e => e.LedgerEntries)
                .HasForeignKey(l => l.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SiteEquipmentLedger>()
                .HasOne(l => l.SourceJob)
                .WithMany()
                .HasForeignKey(l => l.SourceJobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SiteEquipmentLedger>()
                .HasOne(l => l.Batch)
                .WithMany()
                .HasForeignKey(l => l.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SiteEquipmentLedger>()
                .HasOne(l => l.Line)
                .WithMany()
                .HasForeignKey(l => l.LineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure SiteEquipmentSnapshot relationships
            modelBuilder.Entity<SiteEquipmentSnapshot>()
                .HasOne(s => s.Site)
                .WithMany()
                .HasForeignKey(s => s.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SiteEquipmentSnapshot>()
                .HasOne(s => s.EquipmentType)
                .WithMany(e => e.SnapshotEntries)
                .HasForeignKey(s => s.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskDependency>()
                .HasIndex(t => new { t.ParentTaskId, t.PrerequisiteTaskId })
                .IsUnique();

            // Configure relationships and constraints
            modelBuilder.Entity<JobTask>()
                .HasOne(t => t.Job)
                .WithMany(j => j.Tasks)
                .HasForeignKey(t => t.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.ParentTask)
                .WithMany()
                .HasForeignKey(td => td.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.PrerequisiteTask)
                .WithMany()
                .HasForeignKey(td => td.PrerequisiteTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Client
            modelBuilder.Entity<Client>()
                .HasKey(c => c.Id);

            // Configure Site
            modelBuilder.Entity<Site>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Site>()
                .HasOne(s => s.SiteOwner)
                .WithMany()
                .HasForeignKey(s => s.SiteOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure SiteOwner
            modelBuilder.Entity<SiteOwner>()
                .HasKey(so => so.Id);

            // Configure Subcontractor
            modelBuilder.Entity<Subcontractor>()
                .HasKey(s => s.Id);

            // Configure Technician
            modelBuilder.Entity<Technician>()
                .HasKey(t => t.Id);

            // Configure Certification
            modelBuilder.Entity<Certification>()
                .HasKey(c => c.Id);

            // Configure Role
            modelBuilder.Entity<Role>()
                .HasKey(r => r.Id);

            // Seed initial roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Technician" }
            );

            // Seed initial certifications
            modelBuilder.Entity<Certification>().HasData(
                new Certification { Id = 1, Name = "CCNA" },
                new Certification { Id = 2, Name = "CCNP" }
            );

            // Seed initial tool categories
            modelBuilder.Entity<ToolCategory>().HasData(
                new ToolCategory { Id = 1, Name = "Power Tools", Description = "Electric and battery-powered tools" },
                new ToolCategory { Id = 2, Name = "Hand Tools", Description = "Manual tools and equipment" },
                new ToolCategory { Id = 3, Name = "Testing Equipment", Description = "Diagnostic and testing tools" },
                new ToolCategory { Id = 4, Name = "Safety Equipment", Description = "Personal protective equipment" }
            );

            // Seed initial equipment categories
            modelBuilder.Entity<EquipmentCategory>().HasData(
                new EquipmentCategory { Id = 1, Name = "Heavy Machinery", Description = "Large construction and industrial equipment" },
                new EquipmentCategory { Id = 2, Name = "Vehicles", Description = "Transportation and utility vehicles" },
                new EquipmentCategory { Id = 3, Name = "Generators", Description = "Power generation equipment" },
                new EquipmentCategory { Id = 4, Name = "Communication Equipment", Description = "Telecommunications and networking equipment" }
            );

            // Configure JobTaskEquipmentLine relationships
            modelBuilder.Entity<JobTaskEquipmentLine>()
                .HasOne(jtel => jtel.JobTask)
                .WithMany(jt => jt.JobTaskEquipmentLines)
                .HasForeignKey(jtel => jtel.JobTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobTaskEquipmentLine>()
                .HasOne(jtel => jtel.EquipmentLine)
                .WithMany(el => el.JobTaskEquipmentLines)
                .HasForeignKey(jtel => jtel.EquipmentLineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure JobRequirement relationships
            modelBuilder.Entity<JobRequirement>()
                .HasOne(r => r.Job)
                .WithMany(j => j.Requirements)
                .HasForeignKey(r => r.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobRequirement>()
                .HasOne(r => r.EquipmentType)
                .WithMany()
                .HasForeignKey(r => r.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
