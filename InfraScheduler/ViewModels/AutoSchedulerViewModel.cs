using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class AutoSchedulerViewModel : ObservableObject
    {
        private readonly SchedulerService _scheduler;
        private bool _isLoading;

        [ObservableProperty]
        private ObservableCollection<string> schedulingReport = new();

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public AutoSchedulerViewModel(SchedulerService scheduler)
        {
            _scheduler = scheduler;
        }

        [RelayCommand]
        private async Task RunAutoSchedule()
        {
            try
            {
                IsLoading = true;
                SchedulingReport.Clear();
                SchedulingReport.Add("Starting auto-scheduling process...");

                var results = await _scheduler.ScheduleAllUnassignedJobsAsync();
                foreach (var result in results)
                {
                    SchedulingReport.Add(result);
                }

                if (!results.Any())
                {
                    SchedulingReport.Add("No jobs were scheduled. All jobs are already assigned or there are no unassigned jobs.");
                }
                else
                {
                    SchedulingReport.Add($"Auto-scheduling completed. {results.Count} jobs were scheduled.");
                }
            }
            catch (Exception ex)
            {
                SchedulingReport.Add($"❌ Error during auto-scheduling: {ex.Message}");
                MessageBox.Show($"Error during auto-scheduling: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ClearReport()
        {
            SchedulingReport.Clear();
        }
    }
}
