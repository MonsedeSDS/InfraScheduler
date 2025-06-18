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
using System.Windows.Input;

namespace InfraScheduler.ViewModels
{
    public partial class JobTaskViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private int _jobId;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private DateTime _startDate = DateTime.Now;
        private DateTime _endDate = DateTime.Now.AddDays(1);
        private double _progress;
        private string _status = string.Empty;
        private int? _technicianId;
        private JobTask? _selectedTask;
        private Technician? _selectedTechnician;
        private TaskDependency? _selectedDependency;
        private JobTask? _selectedDependentTask;
        private bool _isLoading;
        private ObservableCollection<JobTask> _tasks = new();
        private ObservableCollection<Technician> _technicians = new();
        private ObservableCollection<TaskDependency> _dependencies = new();
        private int? _parentTaskId;
        private int? _prerequisiteTaskId;

        public int JobId
        {
            get => _jobId;
            set
            {
                if (_jobId != value)
                {
                    _jobId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? TechnicianId
        {
            get => _technicianId;
            set
            {
                if (_technicianId != value)
                {
                    _technicianId = value;
                    OnPropertyChanged();
                }
            }
        }

        public JobTask? SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask != value)
                {
                    _selectedTask = value;
                    OnPropertyChanged();
                    if (_selectedTask != null)
                    {
                        LoadTaskDetails(_selectedTask);
                    }
                }
            }
        }

        public Technician? SelectedTechnician
        {
            get => _selectedTechnician;
            set
            {
                if (_selectedTechnician != value)
                {
                    _selectedTechnician = value;
                    OnPropertyChanged();
                    if (_selectedTechnician != null)
                    {
                        TechnicianId = _selectedTechnician.Id;
                    }
                }
            }
        }

        public TaskDependency? SelectedDependency
        {
            get => _selectedDependency;
            set
            {
                if (_selectedDependency != value)
                {
                    _selectedDependency = value;
                    OnPropertyChanged();
                    if (_selectedDependency != null)
                    {
                        LoadDependencyDetails(_selectedDependency);
                    }
                }
            }
        }

        public JobTask? SelectedDependentTask
        {
            get => _selectedDependentTask;
            set
            {
                if (_selectedDependentTask != value)
                {
                    _selectedDependentTask = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public ObservableCollection<JobTask> Tasks
        {
            get => _tasks;
            set
            {
                if (_tasks != value)
                {
                    _tasks = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Technician> Technicians
        {
            get => _technicians;
            set
            {
                if (_technicians != value)
                {
                    _technicians = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<TaskDependency> Dependencies
        {
            get => _dependencies;
            set
            {
                if (_dependencies != value)
                {
                    _dependencies = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? ParentTaskId
        {
            get => _parentTaskId;
            set
            {
                if (_parentTaskId != value)
                {
                    _parentTaskId = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? PrerequisiteTaskId
        {
            get => _prerequisiteTaskId;
            set
            {
                if (_prerequisiteTaskId != value)
                {
                    _prerequisiteTaskId = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand UpdateTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand AddDependencyCommand { get; }
        public ICommand UpdateDependencyCommand { get; }
        public ICommand DeleteDependencyCommand { get; }
        public ICommand ClearCommand { get; }

        public JobTaskViewModel(InfraSchedulerContext context)
        {
            _context = context;
            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            AddTaskCommand = new RelayCommand(async () => await AddTaskAsync());
            UpdateTaskCommand = new RelayCommand(async () => await UpdateTaskAsync());
            DeleteTaskCommand = new RelayCommand(async () => await DeleteTaskAsync());
            AddDependencyCommand = new RelayCommand(async () => await AddDependencyAsync());
            UpdateDependencyCommand = new RelayCommand(async () => await UpdateDependencyAsync());
            DeleteDependencyCommand = new RelayCommand(async () => await DeleteDependencyAsync());
            ClearCommand = new RelayCommand(async () => await ClearFieldsAsync());
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var tasks = await _context.JobTasks
                    .Where(t => t.JobId == JobId)
                    .Include(t => t.Technician)
                    .Include(t => t.Dependencies)
                    .ToListAsync();

                var technicians = await _context.Technicians.ToListAsync();

                Tasks = new ObservableCollection<JobTask>(tasks);
                Technicians = new ObservableCollection<Technician>(technicians);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadTaskDetails(JobTask task)
        {
            Name = task.Name;
            Description = task.Description;
            StartDate = task.StartDate;
            EndDate = task.EndDate;
            Progress = task.Progress;
            Status = task.Status;
            TechnicianId = task.TechnicianId;
        }

        private void LoadDependencyDetails(TaskDependency dependency)
        {
            ParentTaskId = dependency.ParentTaskId;
            PrerequisiteTaskId = dependency.PrerequisiteTaskId;
        }

        private async Task AddTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Please enter a task name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var task = new JobTask
            {
                Name = Name,
                Description = Description,
                StartDate = StartDate,
                EndDate = EndDate,
                Progress = Progress,
                Status = Status,
                JobId = JobId,
                TechnicianId = TechnicianId
            };

            _context.JobTasks.Add(task);
            await _context.SaveChangesAsync();
            await LoadDataAsync();
            await ClearFieldsAsync();
        }

        private async Task UpdateTaskAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task to update.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Please enter a task name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedTask.Name = Name;
            SelectedTask.Description = Description;
            SelectedTask.StartDate = StartDate;
            SelectedTask.EndDate = EndDate;
            SelectedTask.Progress = Progress;
            SelectedTask.Status = Status;
            SelectedTask.TechnicianId = TechnicianId;

            await _context.SaveChangesAsync();
            await LoadDataAsync();
            await ClearFieldsAsync();
        }

        private async Task DeleteTaskAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this task?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _context.JobTasks.Remove(SelectedTask);
                await _context.SaveChangesAsync();
                await LoadDataAsync();
                await ClearFieldsAsync();
            }
        }

        private async Task AddDependencyAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task to add a dependency to.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDependentTask == null)
            {
                MessageBox.Show("Please select a dependent task.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dependency = new TaskDependency
            {
                ParentTaskId = SelectedTask.Id,
                PrerequisiteTaskId = SelectedDependentTask.Id
            };

            _context.TaskDependencies.Add(dependency);
            await _context.SaveChangesAsync();
            await LoadDataAsync();
            await ClearFieldsAsync();
        }

        private async Task UpdateDependencyAsync()
        {
            if (SelectedDependency == null)
            {
                MessageBox.Show("Please select a dependency to update.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDependentTask == null)
            {
                MessageBox.Show("Please select a dependent task.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedDependency.PrerequisiteTaskId = SelectedDependentTask.Id;

            await _context.SaveChangesAsync();
            await LoadDataAsync();
            await ClearFieldsAsync();
        }

        private async Task DeleteDependencyAsync()
        {
            if (SelectedDependency == null)
            {
                MessageBox.Show("Please select a dependency to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this dependency?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _context.TaskDependencies.Remove(SelectedDependency);
                await _context.SaveChangesAsync();
                await LoadDataAsync();
                await ClearFieldsAsync();
            }
        }

        private async Task ClearFieldsAsync()
        {
            Name = string.Empty;
            Description = string.Empty;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddDays(1);
            Progress = 0;
            Status = string.Empty;
            TechnicianId = null;
            SelectedTask = null;
            SelectedTechnician = null;
            SelectedDependency = null;
            SelectedDependentTask = null;
            ParentTaskId = null;
            PrerequisiteTaskId = null;
        }

        public void SetJobId(int jobId)
        {
            JobId = jobId;
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        private async Task AddJobTaskAsync()
        {
            await AddTaskAsync();
        }

        [RelayCommand]
        private async Task UpdateJobTaskAsync()
        {
            await UpdateTaskAsync();
        }

        [RelayCommand]
        private async Task DeleteJobTaskAsync()
        {
            await DeleteTaskAsync();
        }

        [RelayCommand]
        private async Task RemoveDependencyAsync()
        {
            await DeleteDependencyAsync();
        }
    }
}
