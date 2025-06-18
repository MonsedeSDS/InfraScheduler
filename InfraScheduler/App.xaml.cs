using InfraScheduler.Data;
using InfraScheduler.ViewModels;
using InfraScheduler.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace InfraScheduler
{
    public partial class App : Application
    {
        /// <summary>
        /// One shared DbContext instance for the whole desktop session.
        /// </summary>
        public static InfraSchedulerContext Db => ((App)Current)._context;
        public static IServiceProvider ServiceProvider => ((App)Current)._serviceProvider;

        private readonly IServiceProvider _serviceProvider;
        private readonly InfraSchedulerContext _context;
        private NavigationViewModel? _navigationViewModel;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<InfraSchedulerContext>();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Existing startup code
        // ──────────────────────────────────────────────────────────────────────
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _context.Database.EnsureCreated();
            _navigationViewModel = new NavigationViewModel(_context, _serviceProvider);
            MainWindow = new MainWindow(_navigationViewModel);
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _context?.Dispose();
            Db?.Dispose();
        }

        // Register test classes
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<InfraSchedulerContext>(options =>
                options.UseSqlite("Data Source=infrascheduler.db"));

            // Register ViewModels
            services.AddTransient<NavigationViewModel>();
            services.AddTransient<ClientViewModel>();
            services.AddTransient<SubcontractorViewModel>();
            services.AddTransient<SiteViewModel>();
            services.AddTransient<SiteTenantViewModel>();
            services.AddTransient<SiteOwnerViewModel>();
            services.AddTransient<TechnicianViewModel>();
            services.AddTransient<TechnicianAssignmentViewModel>();
            services.AddTransient<MaterialViewModel>();
            services.AddTransient<MaterialResourceViewModel>();
            services.AddTransient<MaterialReservationViewModel>();
            services.AddTransient<JobViewModel>();
            services.AddTransient<JobTaskViewModel>();
            services.AddTransient<TaskDependencyViewModel>();
            services.AddTransient<ResourceCalendarViewModel>();
            services.AddTransient<AllocationViewModel>();
            services.AddTransient<FinancialTransactionViewModel>();
            services.AddTransient<ActivityLogViewModel>();
            services.AddTransient<UserViewModel>();
            services.AddTransient<ScheduleViewModel>();
            services.AddTransient<AutoSchedulerViewModel>();
            services.AddTransient<MaterialAutoReservationViewModel>();
            services.AddTransient<IntegratedSchedulerViewModel>();
            services.AddTransient<GanttViewModel>();

            // Register Views
            services.AddTransient<ClientView>();
            services.AddTransient<SubcontractorView>();
            services.AddTransient<SiteView>();
            services.AddTransient<SiteTenantView>();
            services.AddTransient<SiteOwnerView>();
            services.AddTransient<TechnicianView>();
            services.AddTransient<TechnicianAssignmentView>();
            services.AddTransient<MaterialView>();
            services.AddTransient<MaterialResourceView>();
            services.AddTransient<MaterialReservationView>();
            services.AddTransient<JobView>();
            services.AddTransient<JobTaskView>();
            services.AddTransient<TaskDependencyView>();
            services.AddTransient<ResourceCalendarView>();
            services.AddTransient<AllocationView>();
            services.AddTransient<FinancialTransactionView>();
            services.AddTransient<ActivityLogView>();
            services.AddTransient<UserView>();
            services.AddTransient<ScheduleView>();
            services.AddTransient<AutoSchedulerView>();
            services.AddTransient<MaterialAutoReservationView>();
            services.AddTransient<IntegratedSchedulerView>();
            services.AddTransient<GanttView>();
        }
    }
}
