using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace InfraScheduler.ViewModels
{
    public partial class JobManagementLandingViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _sectionTitle = "Job Management";

        [ObservableProperty]
        private string _sectionDescription = "Manage jobs, tasks, and delivery operations";

        public JobManagementLandingViewModel(InfraSchedulerContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private async Task NavigateToJobs()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowJobViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToJobTasks()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowJobTaskViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToTaskDependencies()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowTaskDependencyViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToJobAcceptance()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowJobAcceptanceViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToJobCloseOut()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowJobCloseOutViewCommand.Execute(null);
        }
    }
} 