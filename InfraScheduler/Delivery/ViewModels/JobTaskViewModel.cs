using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;

namespace InfraScheduler.Delivery.ViewModels
{
    public partial class JobTaskViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly System.Timers.Timer _searchTimer;
        private ObservableCollection<JobTask> _allJobTasks;

        [ObservableProperty]
        private ObservableCollection<JobTask> jobTasks;

        [ObservableProperty]
        private string searchTerm = string.Empty;

        [ObservableProperty]
        private string taskName = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string priority = "Medium";

        [ObservableProperty]
        private JobTask? selectedJobTask;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public JobTaskViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _allJobTasks = new ObservableCollection<JobTask>();
            JobTasks = new ObservableCollection<JobTask>();

            // Initialize search timer
            _searchTimer = new System.Timers.Timer(300); // 300ms delay
            _searchTimer.Elapsed += (s, e) => 
            {
                _searchTimer.Stop();
                Application.Current.Dispatcher.Invoke(() => FilterJobTasks());
            };

            LoadJobTasks();
        }

        private async Task LoadJobTasks()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _allJobTasks.Clear();
                var jobTasks = await _context.JobTasks.Include(jt => jt.Job).ToListAsync();
                foreach (var jobTask in jobTasks)
                {
                    _allJobTasks.Add(jobTask);
                }
                FilterJobTasks();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading job tasks: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSearchTermChanged(string value)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void FilterJobTasks()
        {
            JobTasks.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchTerm) 
                ? _allJobTasks 
                : _allJobTasks.Where(jt => 
                    (jt.TaskName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (jt.Description?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

            foreach (var jobTask in filtered)
            {
                JobTasks.Add(jobTask);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateJobTaskData()) return;

            try
            {
                var jobTask = new JobTask
                {
                    TaskName = TaskName,
                    Description = Description,                    
                };

                _context.JobTasks.Add(jobTask);
                await _context.SaveChangesAsync();
                await LoadJobTasks();
                ClearFields();
                MessageBox.Show("Job task added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving job task: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Update()
        {
            if (SelectedJobTask == null || !ValidateJobTaskData()) return;

            try
            {
                SelectedJobTask.TaskName = TaskName;
                SelectedJobTask.Description = Description;
                // Priority property doesn't exist in JobTask model - removing

                await _context.SaveChangesAsync();
                await LoadJobTasks();
                ClearFields();
                MessageBox.Show("Job task updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating job task: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedJobTask == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete the job task '{SelectedJobTask.TaskName}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.JobTasks.Remove(SelectedJobTask);
                    await _context.SaveChangesAsync();
                    await LoadJobTasks();
                    ClearFields();
                    MessageBox.Show("Job task deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error deleting job task: {ex.Message}";
                    MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateJobTaskData()
        {
            if (string.IsNullOrWhiteSpace(TaskName))
            {
                MessageBox.Show("Task name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearFields()
        {
            TaskName = string.Empty;
            Description = string.Empty;
            Priority = "Medium";
            SelectedJobTask = null;
        }

        partial void OnSelectedJobTaskChanged(JobTask? value)
        {
            if (value != null)
            {
                TaskName = value.TaskName ?? string.Empty;
                Description = value.Description ?? string.Empty;
                                    Priority = "Medium"; // Priority property doesn't exist - using default
            }
        }
    }
}