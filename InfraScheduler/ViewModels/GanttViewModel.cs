using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using InfraScheduler.Commands;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public class GanttViewModel : INotifyPropertyChanged
    {
        private readonly InfraSchedulerContext _context;
        private TimeScale _selectedTimeScale = TimeScale.Days;
        private DateTime _startDate = DateTime.Today;
        private DateTime _endDate = DateTime.Today.AddDays(30);
        private bool _isLoading;
        private ObservableCollection<TaskGroup> _taskGroups = new();
        private string _name = string.Empty;
        private ObservableCollection<GanttTask> _tasks = new();
        private bool _isExpanded;

        public event PropertyChangedEventHandler? PropertyChanged;

        public GanttViewModel(InfraSchedulerContext context)
        {
            _context = context;
            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            UpdateTimeScaleCommand = new RelayCommand<TimeScale>(scale => SelectedTimeScale = scale);
        }

        public GanttViewModel() : this(App.Db) { }

        public ICommand LoadDataCommand { get; }
        public ICommand UpdateTimeScaleCommand { get; }

        public TimeScale SelectedTimeScale
        {
            get => _selectedTimeScale;
            set
            {
                if (_selectedTimeScale != value)
                {
                    _selectedTimeScale = value;
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

        public ObservableCollection<TaskGroup> TaskGroups
        {
            get => _taskGroups;
            set
            {
                if (_taskGroups != value)
                {
                    _taskGroups = value;
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

        public ObservableCollection<GanttTask> Tasks
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

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                var jobs = await _context.Jobs
                    .Include(j => j.Tasks)
                    .ThenInclude(t => t.MaterialRequirements)
                    .Include(j => j.Tasks)
                    .ThenInclude(t => t.Dependencies)
                    .ToListAsync();

                var taskGroups = new ObservableCollection<TaskGroup>();
                foreach (var job in jobs)
                {
                    var group = new TaskGroup
                    {
                        Name = job.Name,
                        Tasks = new ObservableCollection<GanttTask>()
                    };

                    foreach (var task in job.Tasks)
                    {
                        var ganttTask = new GanttTask
                        {
                            Id = task.Id,
                            Name = task.Name,
                            StartDate = task.StartDate,
                            EndDate = task.EndDate,
                            Progress = task.Progress,
                            Status = task.Status,
                            Tooltip = $"Task: {task.Name}\nStatus: {task.Status}\nProgress: {task.Progress}%"
                        };

                        if (task.Dependencies != null)
                        {
                            ganttTask.Dependencies = new ObservableCollection<Dependency>(
                                task.Dependencies.Select(d => new Dependency
                                {
                                    FromTaskId = d.ParentTaskId,
                                    ToTaskId = d.PrerequisiteTaskId
                                })
                            );
                        }

                        group.Tasks.Add(ganttTask);
                    }

                    taskGroups.Add(group);
                }

                TaskGroups = taskGroups;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Gantt data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TaskGroup : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private ObservableCollection<GanttTask> _tasks = new();
        private bool _isExpanded;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public ObservableCollection<GanttTask> Tasks
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

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GanttTask : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _status = string.Empty;
        private string _tooltip = string.Empty;
        private ObservableCollection<Dependency> _dependencies = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Id { get; set; }
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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Progress { get; set; }
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
        public string Tooltip
        {
            get => _tooltip;
            set
            {
                if (_tooltip != value)
                {
                    _tooltip = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<Dependency> Dependencies
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Dependency : INotifyPropertyChanged
    {
        private int _fromTaskId;
        private int _toTaskId;
        private string _pathData = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int FromTaskId
        {
            get => _fromTaskId;
            set
            {
                if (_fromTaskId != value)
                {
                    _fromTaskId = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ToTaskId
        {
            get => _toTaskId;
            set
            {
                if (_toTaskId != value)
                {
                    _toTaskId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PathData
        {
            get => _pathData;
            set
            {
                if (_pathData != value)
                {
                    _pathData = value;
                    OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum TimeScale
    {
        Hours,
        Days,
        Weeks,
        Months
    }
}

