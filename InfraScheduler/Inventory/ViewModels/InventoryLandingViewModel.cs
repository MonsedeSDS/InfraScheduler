using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Inventory.Views;
using InfraScheduler.ViewModels;
using InfraScheduler.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InfraScheduler.Inventory.ViewModels
{
    public partial class InventoryLandingViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationViewModel _navigationViewModel;

        [ObservableProperty]
        private string _sectionTitle = "Inventory Management";

        [ObservableProperty]
        private string _sectionDescription = "Home > Inventory - Manage tools, equipment, consumables, item schedules, and digital warehouse";

        public InventoryLandingViewModel(InfraSchedulerContext context, IServiceProvider serviceProvider, NavigationViewModel navigationViewModel)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _navigationViewModel = navigationViewModel;
        }

        [RelayCommand]
        private async Task NavigateToTools()
        {
            try
            {
                if (_navigationViewModel != null)
                {
                    _navigationViewModel.ShowToolViewCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToToolsManagement()
        {
            try
            {
                if (_navigationViewModel != null)
                {
                    _navigationViewModel.ShowToolViewCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToEquipmentManagement()
        {
            if (_navigationViewModel != null)
            {
                _navigationViewModel.ShowEquipmentViewCommand.Execute(null);
            }
        }

        [RelayCommand]
        private async Task NavigateToConsumablesManagement()
        {
            // TODO: Implement navigation to ConsumablesView when created
        }

        [RelayCommand]
        private async Task NavigateToItemSchedules()
        {
            // TODO: Implement navigation to ItemSchedulesView when created
        }

        [RelayCommand]
        private async Task NavigateToDigitalWarehouse()
        {
            // TODO: Implement navigation to DigitalWarehouseView when created
        }
    }
} 