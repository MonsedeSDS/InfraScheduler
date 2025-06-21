using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace InfraScheduler.Core.ViewModels
{
    public partial class MainLandingViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationViewModel _navigationViewModel;

        [ObservableProperty]
        private string _sectionTitle = "InfraScheduler Dashboard";

        [ObservableProperty]
        private string _sectionDescription = "Welcome to InfraScheduler. Manage your infrastructure projects, inventory, and team resources from this central dashboard.";

        public MainLandingViewModel(IServiceProvider serviceProvider, NavigationViewModel navigationViewModel)
        {
            _serviceProvider = serviceProvider;
            _navigationViewModel = navigationViewModel;
        }

        // Navigation commands that will communicate with NavigationViewModel
        
        [RelayCommand]
        public void NavigateToInventory()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainLandingViewModel: NavigateToInventory called");
                _navigationViewModel.ShowInventoryLandingCommand.Execute(null);
                System.Diagnostics.Debug.WriteLine("MainLandingViewModel: NavigateToInventory completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        public void NavigateToDelivery()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainLandingViewModel: NavigateToDelivery called");
                _navigationViewModel.ShowDeliveryLandingCommand.Execute(null);
                System.Diagnostics.Debug.WriteLine("MainLandingViewModel: NavigateToDelivery completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        public void NavigateToDatabase()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainLandingViewModel: NavigateToDatabase called");
                _navigationViewModel.ShowDatabaseLandingCommand.Execute(null);
                System.Diagnostics.Debug.WriteLine("MainLandingViewModel: NavigateToDatabase completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        public void NavigateToAdmin()
        {
            // TODO: Implement when Admin module is ready
            System.Diagnostics.Debug.WriteLine("Admin navigation not yet implemented");
        }

        [RelayCommand]
        public async System.Threading.Tasks.Task NavigateToAccount()
        {
            // TODO: Implement account navigation when Account module is ready
            await System.Threading.Tasks.Task.CompletedTask;
        }
    }
} 