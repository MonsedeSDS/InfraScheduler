using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace InfraScheduler.Data
{
    public class InfraSchedulerContext : DbContext
    {
        public InfraSchedulerContext(DbContextOptions<InfraSchedulerContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Subcontractor> Subcontractors { get; set; }
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
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<JobTaskTechnician> JobTaskTechnicians { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<MaterialRequirement> MaterialRequirements => Set<MaterialRequirement>();
        public DbSet<TechnicianAssignment> TechnicianAssignments => Set<TechnicianAssignment>();
        public DbSet<ToolMaintenance> ToolMaintenance => Set<ToolMaintenance>();

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
        }
    }
}
