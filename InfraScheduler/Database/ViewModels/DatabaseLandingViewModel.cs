using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.ViewModels;
using InfraScheduler.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace InfraScheduler.Database.ViewModels
{
    public partial class DatabaseLandingViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationViewModel _navigationViewModel;

        [ObservableProperty]
        private string _sectionTitle = "Database Management";

        [ObservableProperty]
        private string _sectionDescription = "Home > Database - Manage sites, clients, technicians, and templates";

        public DatabaseLandingViewModel(InfraSchedulerContext context, IServiceProvider serviceProvider, NavigationViewModel navigationViewModel)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _navigationViewModel = navigationViewModel;
        }

        [RelayCommand]
        private async Task NavigateToSiteInfo()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DatabaseLandingViewModel: NavigateToSiteInfo called");
                if (_navigationViewModel != null)
                {
                    _navigationViewModel.ShowSiteViewCommand.Execute(null);
                }
                System.Diagnostics.Debug.WriteLine("DatabaseLandingViewModel: NavigateToSiteInfo completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToClientInfo()
        {
            try
            {
                if (_navigationViewModel != null)
                {
                    _navigationViewModel.ShowClientViewCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToTechnicianInfo()
        {
            try
            {
                if (_navigationViewModel != null)
                {
                    _navigationViewModel.ShowTechnicianViewCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToSubcontractorInfo()
        {
            try
            {
                if (_navigationViewModel != null)
                {
                    _navigationViewModel.ShowSubcontractorViewCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToProjectTemplate()
        {
            // TODO: Implement navigation to ProjectTemplateView when created
        }

        [RelayCommand]
        private async Task NavigateToJobTemplate()
        {
            // TODO: Implement navigation to JobTemplateView when created
        }

        [RelayCommand]
        private async Task NavigateToJobTaskTemplate()
        {
            try
            {
                if (_navigationViewModel != null)
                {
                    _navigationViewModel.ShowJobTaskViewCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }
    }
} 