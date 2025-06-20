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
    public partial class MainLandingViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _sectionTitle = "Main Dashboard";

        [ObservableProperty]
        private string _sectionDescription = "Welcome to InfraScheduler. Manage your infrastructure projects from this central hub.";

        [ObservableProperty]
        private int _totalJobs;

        [ObservableProperty]
        private int _activeJobs;

        [ObservableProperty]
        private int _totalTechnicians;

        [ObservableProperty]
        private int _totalEquipment;

        public ObservableCollection<QuickActionCard> QuickActions { get; set; }

        public MainLandingViewModel(InfraSchedulerContext context, IServiceProvider serviceProvider)
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
                Title = "Dashboard",
                Description = "View key metrics, live job counts, and system alerts",
                Icon = "📊",
                Command = new RelayCommand(() => NavigateToDashboard())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Recent Jobs",
                Description = "Quick access to your most recent job activities",
                Icon = "📋",
                Command = new RelayCommand(() => NavigateToRecentJobs())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "System Status",
                Description = "Check system health and performance metrics",
                Icon = "⚡",
                Command = new RelayCommand(() => NavigateToSystemStatus())
            });
        }

        private void LoadMetrics()
        {
            try
            {
                TotalJobs = _context.Jobs?.Count() ?? 0;
                ActiveJobs = _context.Jobs?.Count(j => j.Status == "Active") ?? 0;
                TotalTechnicians = _context.Technicians?.Count() ?? 0;
                TotalEquipment = _context.Equipment?.Count() ?? 0;
            }
            catch (Exception ex)
            {
                // Handle database connection issues gracefully
                TotalJobs = 0;
                ActiveJobs = 0;
                TotalTechnicians = 0;
                TotalEquipment = 0;
            }
        }

        [RelayCommand]
        private async Task NavigateToDashboard()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowWorkflowDashboardViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToRecentJobs()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowJobViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToSystemStatus()
        {
            // For now, navigate to dashboard as system status view doesn't exist yet
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowWorkflowDashboardViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task RefreshMetrics()
        {
            LoadMetrics();
        }
    }

    public class QuickActionCard
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public RelayCommand Command { get; set; }
    }
} 