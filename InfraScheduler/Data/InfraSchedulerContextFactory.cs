using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace InfraScheduler.Data
{
    public class InfraSchedulerContextFactory : IDesignTimeDbContextFactory<InfraSchedulerContext>
    {
        public InfraSchedulerContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<InfraSchedulerContext>();

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new InfraSchedulerContext(optionsBuilder.Options);
        }
    }
}
