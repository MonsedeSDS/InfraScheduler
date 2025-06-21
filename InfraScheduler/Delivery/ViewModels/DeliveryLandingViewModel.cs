using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input; 
using InfraScheduler.Data;
using InfraScheduler.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace InfraScheduler.Delivery.ViewModels
{
    public partial class DeliveryLandingViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _sectionTitle = "Delivery Management";

        [ObservableProperty]
        private string _sectionDescription = "Home > Delivery - Manage projects, deliveries, and track records";

        public DeliveryLandingViewModel(InfraSchedulerContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private async Task NavigateToProjectsList()
        {
            try
            {
                var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
                if (navigationViewModel != null)
                {
                    navigationViewModel.ShowProjectDetailViewCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToDeliveryManagement()
        {
            try
            {
                var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
                if (navigationViewModel != null)
                {
                    navigationViewModel.ShowLogisticsShippingViewCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToTrackRecord()
        {
            try
            {
                var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
                if (navigationViewModel != null)
                {
                    navigationViewModel.ShowReceivingViewCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToScheduling()
        {
            // TODO: Implement navigation to SchedulingView when created
        }

        [RelayCommand]
        private async Task NavigateToResources()
        {
            // TODO: Implement navigation to ResourcesView when created
        }
    }
} 