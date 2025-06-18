using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class IntegratedSchedulerViewModel : ObservableObject
    {
        private readonly SchedulerService _scheduler;
        private readonly InfraSchedulerContext _context;
        private bool _isLoading;

        public ObservableCollection<JobTask> JobTasks { get; set; } = new();
        public ObservableCollection<string> SchedulingReport { get; set; } = new();

        [ObservableProperty]
        private JobTask? selectedJobTask;

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

        public IntegratedSchedulerViewModel(SchedulerService scheduler, InfraSchedulerContext context)
        {
            _scheduler = scheduler;
            _context = context;
            LoadJobTasks();
        }

        private async Task LoadJobTasks()
        {
            try
            {
                IsLoading = true;
                JobTasks.Clear();
                var tasks = await _context.JobTasks
                    .Include(t => t.Job)
                    .Include(t => t.Technician)
                    .ToListAsync();
                foreach (var task in tasks)
                {
                    JobTasks.Add(task);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading job tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RunIntegratedAnalysis()
        {
            try
            {
                IsLoading = true;
                SchedulingReport.Clear();

                if (SelectedJobTask == null)
                {
                    SchedulingReport.Add("❌ Please select a Job Task.");
                    return;
                }

                SchedulingReport.Add($"Analyzing task: {SelectedJobTask.Name}...");
                var results = await _scheduler.AnalyzeTaskAsync(SelectedJobTask);

                if (!results.Any())
                {
                    SchedulingReport.Add("✅ Task is fully schedulable.");
                }
                else
                {
                    foreach (var issue in results)
                    {
                        SchedulingReport.Add($"⚠️ {issue}");
                    }
                }
            }
            catch (Exception ex)
            {
                SchedulingReport.Add($"❌ Error during analysis: {ex.Message}");
                MessageBox.Show($"Error during analysis: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        [RelayCommand]
        private async Task RefreshTasks()
        {
            await LoadJobTasks();
        }
    }
}
