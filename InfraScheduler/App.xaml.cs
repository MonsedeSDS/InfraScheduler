using InfraScheduler.Data;
using InfraScheduler.ViewModels;
using InfraScheduler.Views;
using InfraScheduler.Core.ViewModels;
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
        private InfraScheduler.ViewModels.NavigationViewModel? _navigationViewModel;

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
            
            // Create NavigationViewModel directly to ensure the constructor with CurrentView initialization is called
            try
            {
                _navigationViewModel = new InfraScheduler.ViewModels.NavigationViewModel(_context, _serviceProvider);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"App.xaml.cs: Error creating NavigationViewModel: {ex.Message}");
                return;
            }
            
            // Create MainWindow manually and set its DataContext
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
            services.AddTransient<InfraScheduler.ViewModels.NavigationViewModel>();
            services.AddTransient<InfraScheduler.Database.ViewModels.ClientViewModel>();
            services.AddTransient<InfraScheduler.Database.ViewModels.SubcontractorViewModel>();
            services.AddTransient<InfraScheduler.Database.ViewModels.SiteViewModel>();
            services.AddTransient<SiteTenantViewModel>();
            services.AddTransient<SiteOwnerViewModel>();
            services.AddTransient<InfraScheduler.Database.ViewModels.TechnicianViewModel>();
            services.AddTransient<TechnicianAssignmentViewModel>();
            services.AddTransient<InfraScheduler.Inventory.ViewModels.ToolViewModel>();
            services.AddTransient<ToolCategoryViewModel>();
            services.AddTransient<ToolAssignmentViewModel>();
            services.AddTransient<ToolMaintenanceViewModel>();
            services.AddTransient<InfraScheduler.Inventory.ViewModels.EquipmentViewModel>();
            services.AddTransient<InfraScheduler.Inventory.ViewModels.EquipmentCategoryViewModel>();
            services.AddTransient<WorkflowDashboardViewModel>();
            services.AddTransient<InfraScheduler.Delivery.ViewModels.JobViewModel>();
            services.AddTransient<InfraScheduler.Delivery.ViewModels.JobTaskViewModel>();
            services.AddTransient<TaskDependencyViewModel>();
            services.AddTransient<FinancialTransactionViewModel>();
            services.AddTransient<ActivityLogViewModel>();
            services.AddTransient<InfraScheduler.Admin.ViewModels.UserViewModel>();
            services.AddTransient<JobAcceptanceViewModel>();
            services.AddTransient<JobCloseOutViewModel>();
            services.AddTransient<SiteEquipmentInventoryViewModel>();

            // Register Landing Page ViewModels
            services.AddTransient<InfraScheduler.Delivery.ViewModels.DeliveryLandingViewModel>();
            services.AddTransient<InfraScheduler.Inventory.ViewModels.InventoryLandingViewModel>();
            services.AddTransient<InfraScheduler.Database.ViewModels.DatabaseLandingViewModel>();

            services.AddTransient<ProjectManagementViewModel>();
            services.AddTransient<InfraScheduler.Delivery.ViewModels.ProjectDetailViewModel>();

            // Register Views
            services.AddTransient<InfraScheduler.Core.Views.NavigationView>();
            services.AddTransient<InfraScheduler.Database.Views.ClientView>();
            services.AddTransient<InfraScheduler.Database.Views.SubcontractorView>();
            services.AddTransient<InfraScheduler.Database.Views.SiteView>();
            services.AddTransient<SiteTenantView>();
            services.AddTransient<SiteOwnerView>();
            services.AddTransient<InfraScheduler.Database.Views.TechnicianView>();
            services.AddTransient<TechnicianAssignmentView>();
            services.AddTransient<InfraScheduler.Inventory.Views.ToolView>();
            services.AddTransient<ToolCategoryView>();
            services.AddTransient<ToolAssignmentView>();
            services.AddTransient<ToolMaintenanceView>();
            services.AddTransient<InfraScheduler.Inventory.Views.EquipmentView>();
            services.AddTransient<InfraScheduler.Inventory.Views.EquipmentCategoryView>();
            services.AddTransient<WorkflowDashboardView>();
            services.AddTransient<InfraScheduler.Delivery.Views.JobView>();
            services.AddTransient<InfraScheduler.Delivery.Views.JobTaskView>();
            services.AddTransient<TaskDependencyView>();
            services.AddTransient<FinancialTransactionView>();
            services.AddTransient<InfraScheduler.Admin.Views.UserView>();
            services.AddTransient<JobAcceptanceView>();
            services.AddTransient<JobCloseOutView>();
            services.AddTransient<SiteEquipmentInventoryView>();

            // Register Landing Page Views
            services.AddTransient<InfraScheduler.Core.Views.MainLandingView>();
            services.AddTransient<InfraScheduler.Delivery.Views.DeliveryLandingView>();
            services.AddTransient<InfraScheduler.Inventory.Views.InventoryLandingView>();
            services.AddTransient<InfraScheduler.Database.Views.DatabaseLandingView>();
            services.AddTransient<InfraScheduler.Delivery.Views.ProjectDetailView>();
        }
    }
}
