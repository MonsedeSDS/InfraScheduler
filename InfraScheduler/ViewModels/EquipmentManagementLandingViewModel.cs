using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace InfraScheduler.ViewModels
{
    public partial class EquipmentManagementLandingViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _sectionTitle = "Equipment Management";

        [ObservableProperty]
        private string _sectionDescription = "Manage equipment inventory, tools, and asset tracking. Monitor equipment status and maintenance.";

        [ObservableProperty]
        private int _totalEquipment;

        [ObservableProperty]
        private int _availableEquipment;

        [ObservableProperty]
        private int _inUseEquipment;

        [ObservableProperty]
        private int _maintenanceEquipment;

        [ObservableProperty]
        private int _totalTools;

        public ObservableCollection<QuickActionCard> QuickActions { get; set; }

        public EquipmentManagementLandingViewModel(InfraSchedulerContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            QuickActions = new ObservableCollection<QuickActionCard>();
            LoadQuickActions();
            LoadMetrics();
        }

        private void LoadQuickActions()
        {
            QuickActions.Clear();
            
            QuickActions.Add(new QuickActionCard
            {
                Title = "Equipment",
                Description = "View and manage all equipment inventory",
                Icon = "🔧",
                Command = new RelayCommand(() => NavigateToEquipment())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Equipment Categories",
                Description = "Manage equipment categories and classifications",
                Icon = "📂",
                Command = new RelayCommand(() => NavigateToEquipmentCategories())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Tools",
                Description = "Manage tools and tool assignments",
                Icon = "🛠️",
                Command = new RelayCommand(() => NavigateToTools())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Tool Categories",
                Description = "Organize tools by categories",
                Icon = "📁",
                Command = new RelayCommand(() => NavigateToToolCategories())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Tool Maintenance",
                Description = "Track tool maintenance and repairs",
                Icon = "🔧",
                Command = new RelayCommand(() => NavigateToToolMaintenance())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Tool Assignments",
                Description = "Manage tool assignments to technicians",
                Icon = "👷",
                Command = new RelayCommand(() => NavigateToToolAssignments())
            });
        }

        private void LoadMetrics()
        {
            try
            {
                TotalEquipment = _context.Equipment?.Count() ?? 0;
                AvailableEquipment = _context.Equipment?.Count(e => e.Status == "Available") ?? 0;
                InUseEquipment = _context.Equipment?.Count(e => e.Status == "In Use") ?? 0;
                MaintenanceEquipment = _context.Equipment?.Count(e => e.Status == "Maintenance") ?? 0;
                TotalTools = _context.Tools?.Count() ?? 0;
            }
            catch (Exception ex)
            {
                // Handle database connection issues gracefully
                TotalEquipment = 0;
                AvailableEquipment = 0;
                InUseEquipment = 0;
                MaintenanceEquipment = 0;
                TotalTools = 0;
            }
        }

        [RelayCommand]
        private async Task NavigateToEquipment()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowEquipmentViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToEquipmentCategories()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowEquipmentCategoryViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToTools()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowToolViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToToolCategories()
        {
            // This would need to be implemented when ToolCategoryView is created
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            // navigationViewModel?.ShowToolCategoryViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToToolMaintenance()
        {
            // This would need to be implemented when ToolMaintenanceView is created
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            // navigationViewModel?.ShowToolMaintenanceViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToToolAssignments()
        {
            // This would need to be implemented when ToolAssignmentView is created
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            // navigationViewModel?.ShowToolAssignmentViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task RefreshMetrics()
        {
            LoadMetrics();
        }
    }
} 