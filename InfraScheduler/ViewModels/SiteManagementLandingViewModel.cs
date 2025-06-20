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
    public partial class SiteManagementLandingViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _sectionTitle = "Site Management";

        [ObservableProperty]
        private string _sectionDescription = "Manage sites, clients, and location data. Track site ownership and tenant information.";

        [ObservableProperty]
        private int _totalSites;

        [ObservableProperty]
        private int _totalClients;

        [ObservableProperty]
        private int _totalSiteOwners;

        [ObservableProperty]
        private int _totalSiteTenants;

        [ObservableProperty]
        private int _totalSubcontractors;

        public ObservableCollection<QuickActionCard> QuickActions { get; set; }

        public SiteManagementLandingViewModel(InfraSchedulerContext context, IServiceProvider serviceProvider)
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
                Title = "Sites",
                Description = "View and manage all sites",
                Icon = "🏢",
                Command = new RelayCommand(() => NavigateToSites())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Clients",
                Description = "Manage client information",
                Icon = "👥",
                Command = new RelayCommand(() => NavigateToClients())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Site Owners",
                Description = "Manage site ownership information",
                Icon = "🏠",
                Command = new RelayCommand(() => NavigateToSiteOwners())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Site Tenants",
                Description = "Manage tenant information",
                Icon = "🏘️",
                Command = new RelayCommand(() => NavigateToSiteTenants())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Subcontractors",
                Description = "Manage subcontractor information",
                Icon = "👷",
                Command = new RelayCommand(() => NavigateToSubcontractors())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Site Equipment",
                Description = "Track equipment at specific sites",
                Icon = "🔧",
                Command = new RelayCommand(() => NavigateToSiteEquipment())
            });
        }

        private void LoadMetrics()
        {
            try
            {
                TotalSites = _context.Sites?.Count() ?? 0;
                TotalClients = _context.Clients?.Count() ?? 0;
                TotalSiteOwners = _context.SiteOwners?.Count() ?? 0;
                TotalSiteTenants = _context.SiteTenants?.Count() ?? 0;
                TotalSubcontractors = _context.Subcontractors?.Count() ?? 0;
            }
            catch (Exception ex)
            {
                // Handle database connection issues gracefully
                TotalSites = 0;
                TotalClients = 0;
                TotalSiteOwners = 0;
                TotalSiteTenants = 0;
                TotalSubcontractors = 0;
            }
        }

        [RelayCommand]
        private async Task NavigateToSites()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowSiteViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToClients()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowClientViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToSiteOwners()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowSiteOwnerViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToSiteTenants()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowSiteTenantViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToSubcontractors()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowSubcontractorViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToSiteEquipment()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowSiteEquipmentInventoryViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task RefreshMetrics()
        {
            LoadMetrics();
        }
    }
} 