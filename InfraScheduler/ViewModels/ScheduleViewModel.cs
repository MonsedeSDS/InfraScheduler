using CommunityToolkit.Mvvm.ComponentModel;
using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using InfraScheduler.Commands;

namespace InfraScheduler.ViewModels
{
    public partial class ScheduleViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly SchedulerService _scheduler;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _searchTerm = string.Empty;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private JobTask? _selectedTask;

        [ObservableProperty]
        private ObservableCollection<JobTask> _jobTasks = new();

        [ObservableProperty]
        private ObservableCollection<ScheduleSlot> _scheduleSlots = new();

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _showStatusMessage;

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand AssignTechnicianCommand { get; }
        public ICommand UnassignTechnicianCommand { get; }
        public ICommand LockSlotCommand { get; }
        public ICommand UnlockSlotCommand { get; }

        public ScheduleViewModel(InfraSchedulerContext context, SchedulerService scheduler)
        {
            _context = context;
            _scheduler = scheduler;

            LoadCommand = new RelayCommand(async () =>
            {
                await LoadDataAsync();
            });
            RefreshCommand = new RelayCommand(async () =>
            {
                await LoadDataAsync();
            });
            ClearCommand = new RelayCommand(async () =>
            {
                await Task.Run(() => ClearSelection());
            });
            AssignTechnicianCommand = new RelayCommand<ScheduleSlot>(async slot =>
            {
                await AssignTechnicianAsync(slot);
            });
            UnassignTechnicianCommand = new RelayCommand<ScheduleSlot>(async slot =>
            {
                await UnassignTechnicianAsync(slot);
            });
            LockSlotCommand = new RelayCommand<ScheduleSlot>(async slot =>
            {
                await LockSlotAsync(slot);
            });
            UnlockSlotCommand = new RelayCommand<ScheduleSlot>(async slot =>
            {
                await UnlockSlotAsync(slot);
            });

            // Subscribe to property changes
            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SearchTerm))
                {
                    FilterTasks();
                }
                else if (e.PropertyName == nameof(SelectedDate))
                {
                    _ = LoadDataAsync();
                }
            };
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                ShowStatusMessage = false;

                // Load tasks for the selected date
                var query = _context.JobTasks
                    .Include(t => t.Job)
                    .Include(t => t.Technician)
                    .Include(t => t.MaterialRequirements)
                        .ThenInclude(mr => mr.Material)
                    .Include(t => t.Dependencies)
                    .Where(t => t.StartDate.Date <= SelectedDate && t.EndDate.Date >= SelectedDate);

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    query = query.Where(t =>
                        t.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        t.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        t.Status.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        t.Job.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (t.Technician != null &&
                            (t.Technician.FirstName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                             t.Technician.LastName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))));
                }

                var tasks = await query.ToListAsync();
                JobTasks = new ObservableCollection<JobTask>(tasks);

                // Load schedule slots for the selected date
                var slots = await _context.ScheduleSlots
                    .Include(s => s.JobTask)
                    .Include(s => s.Technician)
                    .Where(s => s.ScheduledStart.Date <= SelectedDate && s.ScheduledEnd.Date >= SelectedDate)
                    .ToListAsync();

                ScheduleSlots = new ObservableCollection<ScheduleSlot>(slots);

                ShowStatusMessage = true;
                StatusMessage = $"Loaded {tasks.Count} tasks and {slots.Count} schedule slots for {SelectedDate:yyyy-MM-dd}";
            }
            catch (Exception ex)
            {
                ShowStatusMessage = true;
                StatusMessage = $"Error loading schedule data: {ex.Message}";
                MessageBox.Show(StatusMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterTasks()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                _ = LoadDataAsync();
                return;
            }

            var filteredTasks = JobTasks.Where(t =>
                t.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Status.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Job.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                (t.Technician != null &&
                    (t.Technician.FirstName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                     t.Technician.LastName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)))
            ).ToList();

            JobTasks = new ObservableCollection<JobTask>(filteredTasks);
            ShowStatusMessage = true;
            StatusMessage = $"Filtered to {filteredTasks.Count} tasks matching '{SearchTerm}'";
        }

        private void ClearSelection()
        {
            SelectedTask = null;
            SearchTerm = string.Empty;
            _ = LoadDataAsync();
        }

        private async Task AssignTechnicianAsync(ScheduleSlot? slot)
        {
            if (slot == null || slot.JobTask == null)
            {
                MessageBox.Show("Please select a schedule slot to assign a technician.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                IsLoading = true;
                ShowStatusMessage = false;

                var technician = await _scheduler.FindEarliestAvailableTechnicianAsync(slot.JobTask);
                if (technician != null)
                {
                    var result = await _scheduler.AssignTechnicianAsync(slot.JobTask, technician.Id);
                    if (result)
                    {
                        await LoadDataAsync();
                        ShowStatusMessage = true;
                        StatusMessage = $"Successfully assigned {technician.FirstName} {technician.LastName} to task '{slot.JobTask.Name}'";
                    }
                    else
                    {
                        ShowStatusMessage = true;
                        StatusMessage = "Failed to assign technician. Please try again.";
                    }
                }
                else
                {
                    ShowStatusMessage = true;
                    StatusMessage = "No suitable technician found for this task.";
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage = true;
                StatusMessage = $"Error assigning technician: {ex.Message}";
                MessageBox.Show(StatusMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UnassignTechnicianAsync(ScheduleSlot? slot)
        {
            if (slot == null || slot.JobTask == null)
            {
                MessageBox.Show("Please select a schedule slot to unassign a technician.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                IsLoading = true;
                ShowStatusMessage = false;

                var result = await _scheduler.UnassignTechnicianAsync(slot.JobTask);
                if (result)
                {
                    await LoadDataAsync();
                    ShowStatusMessage = true;
                    StatusMessage = $"Successfully unassigned technician from task '{slot.JobTask.Name}'";
                }
                else
                {
                    ShowStatusMessage = true;
                    StatusMessage = "Failed to unassign technician. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage = true;
                StatusMessage = $"Error unassigning technician: {ex.Message}";
                MessageBox.Show(StatusMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LockSlotAsync(ScheduleSlot? slot)
        {
            if (slot == null)
            {
                MessageBox.Show("Please select a schedule slot to lock.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                IsLoading = true;
                ShowStatusMessage = false;

                var result = await _scheduler.LockSlotAsync(slot);
                if (result)
                {
                    await LoadDataAsync();
                    ShowStatusMessage = true;
                    StatusMessage = $"Successfully locked schedule slot for {slot.ScheduledStart:yyyy-MM-dd}";
                }
                else
                {
                    ShowStatusMessage = true;
                    StatusMessage = "Failed to lock schedule slot. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage = true;
                StatusMessage = $"Error locking schedule slot: {ex.Message}";
                MessageBox.Show(StatusMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UnlockSlotAsync(ScheduleSlot? slot)
        {
            if (slot == null)
            {
                MessageBox.Show("Please select a schedule slot to unlock.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                IsLoading = true;
                ShowStatusMessage = false;

                var result = await _scheduler.UnlockSlotAsync(slot);
                if (result)
                {
                    await LoadDataAsync();
                    ShowStatusMessage = true;
                    StatusMessage = $"Successfully unlocked schedule slot for {slot.ScheduledStart:yyyy-MM-dd}";
                }
                else
                {
                    ShowStatusMessage = true;
                    StatusMessage = "Failed to unlock schedule slot. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage = true;
                StatusMessage = $"Error unlocking schedule slot: {ex.Message}";
                MessageBox.Show(StatusMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 