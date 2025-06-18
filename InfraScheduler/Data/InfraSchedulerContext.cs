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
        public DbSet<Material> Materials { get; set; }
        public DbSet<MaterialRequirement> MaterialRequirements { get; set; }
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<ResourceCategory> ResourceCategories { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<ResourceCalendar> ResourceCalendars { get; set; }
        public DbSet<Allocation> Allocations { get; set; }
        public DbSet<MaterialResource> MaterialResources { get; set; }
        public DbSet<JobTaskTechnician> JobTaskTechnicians { get; set; }
        public DbSet<JobTaskMaterial> JobTaskMaterials { get; set; }
        public DbSet<TechnicianAssignment> TechnicianAssignments { get; set; }
        public DbSet<MaterialReservation> MaterialReservations { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }
        public DbSet<ScheduleSlot> ScheduleSlots { get; set; }
        public DbSet<MaterialInventory> MaterialInventory { get; set; }
        public DbSet<MaterialDelivery> MaterialDeliveries { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<Role> Roles { get; set; }

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

            // Inheritance map
            modelBuilder.Entity<Resource>()
                        .HasDiscriminator<string>("ResourceType");

            // Seed reference data
            modelBuilder.Entity<ResourceCategory>().HasData(
                new ResourceCategory { Id = 1, Name = "Technician" },
                new ResourceCategory { Id = 2, Name = "Vehicle" },
                new ResourceCategory { Id = 3, Name = "Tool" },
                new ResourceCategory { Id = 4, Name = "Budget" }
            );

            modelBuilder.Entity<TaskDependency>()
                .HasIndex(t => new { t.ParentTaskId, t.PrerequisiteTaskId })
                .IsUnique();

            // Configure relationships and constraints
            modelBuilder.Entity<JobTask>()
                .HasOne(t => t.Job)
                .WithMany(j => j.Tasks)
                .HasForeignKey(t => t.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaterialRequirement>()
                .HasOne(r => r.JobTask)
                .WithMany(t => t.MaterialRequirements)
                .HasForeignKey(r => r.JobTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaterialRequirement>()
                .HasOne(r => r.Material)
                .WithMany()
                .HasForeignKey(r => r.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<MaterialInventory>()
                .HasOne(i => i.Material)
                .WithMany()
                .HasForeignKey(i => i.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaterialDelivery>()
                .HasOne(d => d.Material)
                .WithMany()
                .HasForeignKey(d => d.MaterialId)
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
        }
    }
}
