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
        private Material? _selectedMaterial;
        private double _quantity;
        private TaskDependency? _selectedDependency;
        private JobTask? _selectedDependentTask;
        private string _forecastResult = string.Empty;
        private bool _isLoading;
        private ObservableCollection<JobTask> _tasks = new();
        private ObservableCollection<Technician> _technicians = new();
        private ObservableCollection<Material> _materials = new();
        private ObservableCollection<MaterialRequirement> _materialRequirements = new();
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

        public Material? SelectedMaterial
        {
            get => _selectedMaterial;
            set
            {
                if (_selectedMaterial != value)
                {
                    _selectedMaterial = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
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

        public string ForecastResult
        {
            get => _forecastResult;
            set
            {
                if (_forecastResult != value)
                {
                    _forecastResult = value;
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
                _tasks = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Technician> Technicians
        {
            get => _technicians;
            set
            {
                _technicians = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Material> Materials
        {
            get => _materials;
            set
            {
                _materials = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MaterialRequirement> MaterialRequirements
        {
            get => _materialRequirements;
            set
            {
                _materialRequirements = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TaskDependency> Dependencies
        {
            get => _dependencies;
            set
            {
                _dependencies = value;
                OnPropertyChanged();
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
        public ICommand AddMaterialRequirementCommand { get; }
        public ICommand UpdateMaterialRequirementCommand { get; }
        public ICommand DeleteMaterialRequirementCommand { get; }
        public ICommand AddDependencyCommand { get; }
        public ICommand UpdateDependencyCommand { get; }
        public ICommand DeleteDependencyCommand { get; }
        public ICommand ForecastCommand { get; }
        public ICommand ClearCommand { get; }

        public JobTaskViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            AddTaskCommand = new AsyncRelayCommand(AddTaskAsync);
            UpdateTaskCommand = new AsyncRelayCommand(UpdateTaskAsync);
            DeleteTaskCommand = new AsyncRelayCommand(DeleteTaskAsync);
            AddMaterialRequirementCommand = new AsyncRelayCommand(AddMaterialRequirementAsync);
            UpdateMaterialRequirementCommand = new AsyncRelayCommand(UpdateMaterialRequirementAsync);
            DeleteMaterialRequirementCommand = new AsyncRelayCommand(DeleteMaterialRequirementAsync);
            AddDependencyCommand = new AsyncRelayCommand(AddDependencyAsync);
            UpdateDependencyCommand = new AsyncRelayCommand(UpdateDependencyAsync);
            DeleteDependencyCommand = new AsyncRelayCommand(DeleteDependencyAsync);
            ForecastCommand = new AsyncRelayCommand(ForecastAsync);
            ClearCommand = new AsyncRelayCommand(ClearFieldsAsync);
        }

        private async Task LoadDataAsync()
        {
            if (_jobId <= 0) return;

            var tasks = await _context.JobTasks
                .Include(t => t.Technician)
                .Include(t => t.MaterialRequirements)
                    .ThenInclude(mr => mr.Material)
                .Include(t => t.Dependencies)
                    .ThenInclude(d => d.ParentTask)
                .Include(t => t.Dependencies)
                    .ThenInclude(d => d.PrerequisiteTask)
                .Where(t => t.JobId == _jobId)
                .ToListAsync();

            Tasks.Clear();
            foreach (var task in tasks)
            {
                Tasks.Add(task);
            }

            var technicians = await _context.Technicians.ToListAsync();
            Technicians.Clear();
            foreach (var technician in technicians)
            {
                Technicians.Add(technician);
            }

            var materials = await _context.Materials.ToListAsync();
            Materials.Clear();
            foreach (var material in materials)
            {
                Materials.Add(material);
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
                MessageBox.Show("Please enter a task name.");
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
                JobId = _jobId,
                TechnicianId = TechnicianId
            };

            _context.JobTasks.Add(task);
            await _context.SaveChangesAsync();
            await LoadDataAsync();
        }

        private async Task UpdateTaskAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task to update.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Please enter a task name.");
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
        }

        private async Task DeleteTaskAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task to delete.");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this task?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _context.JobTasks.Remove(SelectedTask);
                await _context.SaveChangesAsync();
                await LoadDataAsync();
            }
        }

        private async Task AddDependencyAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            if (ParentTaskId == null || PrerequisiteTaskId == null)
            {
                MessageBox.Show("Please select both parent and prerequisite tasks.");
                return;
            }

            var dependency = new TaskDependency
            {
                ParentTaskId = ParentTaskId.Value,
                PrerequisiteTaskId = PrerequisiteTaskId.Value
            };

            _context.TaskDependencies.Add(dependency);
            await _context.SaveChangesAsync();
            await LoadDataAsync();
        }

        private async Task UpdateDependencyAsync()
        {
            if (SelectedDependency == null)
            {
                MessageBox.Show("Please select a dependency to update.");
                return;
            }

            if (ParentTaskId == null || PrerequisiteTaskId == null)
            {
                MessageBox.Show("Please select both parent and prerequisite tasks.");
                return;
            }

            SelectedDependency.ParentTaskId = ParentTaskId.Value;
            SelectedDependency.PrerequisiteTaskId = PrerequisiteTaskId.Value;

            await _context.SaveChangesAsync();
            await LoadDataAsync();
        }

        private async Task DeleteDependencyAsync()
        {
            if (SelectedDependency == null)
            {
                MessageBox.Show("Please select a dependency to delete.");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this dependency?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _context.TaskDependencies.Remove(SelectedDependency);
                await _context.SaveChangesAsync();
                await LoadDataAsync();
            }
        }

        private async Task AddMaterialRequirementAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            if (SelectedMaterial == null)
            {
                MessageBox.Show("Please select a material.");
                return;
            }

            if (Quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.");
                return;
            }

            var requirement = new MaterialRequirement
            {
                JobTaskId = SelectedTask.Id,
                MaterialId = SelectedMaterial.Id,
                Quantity = Quantity,
                Unit = "pcs" // Default unit
            };

            _context.MaterialRequirements.Add(requirement);
            await _context.SaveChangesAsync();
            await LoadDataAsync();
        }

        private async Task UpdateMaterialRequirementAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            if (SelectedMaterial == null)
            {
                MessageBox.Show("Please select a material.");
                return;
            }

            if (Quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.");
                return;
            }

            var requirement = await _context.MaterialRequirements
                .FirstOrDefaultAsync(r => r.JobTaskId == SelectedTask.Id && r.MaterialId == SelectedMaterial.Id);

            if (requirement != null)
            {
                requirement.Quantity = Quantity;
                await _context.SaveChangesAsync();
                await LoadDataAsync();
            }
        }

        private async Task DeleteMaterialRequirementAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            if (SelectedMaterial == null)
            {
                MessageBox.Show("Please select a material.");
                return;
            }

            var requirement = await _context.MaterialRequirements
                .FirstOrDefaultAsync(r => r.JobTaskId == SelectedTask.Id && r.MaterialId == SelectedMaterial.Id);

            if (requirement != null)
            {
                _context.MaterialRequirements.Remove(requirement);
                await _context.SaveChangesAsync();
                await LoadDataAsync();
            }
        }

        private async Task ForecastAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            var dependencies = await _context.TaskDependencies
                .Where(d => d.ParentTaskId == SelectedTask.Id || d.PrerequisiteTaskId == SelectedTask.Id)
                .ToListAsync();

            if (dependencies.Any())
            {
                ForecastResult = $"Task has {dependencies.Count} dependencies.";
            }
            else
            {
                ForecastResult = "Task has no dependencies.";
            }
        }

        private async Task ForecastMaterialAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            var requirements = await _context.MaterialRequirements
                .Where(r => r.JobTaskId == SelectedTask.Id)
                .ToListAsync();

            if (requirements.Any())
            {
                ForecastResult = $"Task requires {requirements.Count} different materials.";
            }
            else
            {
                ForecastResult = "No material requirements found.";
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
            SelectedMaterial = null;
            Quantity = 0;
            SelectedDependency = null;
            SelectedDependentTask = null;
            ForecastResult = string.Empty;
            await Task.CompletedTask;
        }

        public void SetJobId(int jobId)
        {
            if (_jobId != jobId)
            {
                _jobId = jobId;
                OnPropertyChanged(nameof(JobId));
                _ = LoadDataAsync();
            }
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

        [RelayCommand]
        private async Task ForecastTaskAsync()
        {
            await ForecastAsync();
        }

        [RelayCommand]
        private async Task CheckConflictsAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            var conflicts = await _context.TaskDependencies
                .Where(d => d.ParentTaskId == SelectedTask.Id || d.PrerequisiteTaskId == SelectedTask.Id)
                .ToListAsync();

            if (conflicts.Any())
            {
                ForecastResult = $"Found {conflicts.Count} potential conflicts.";
            }
            else
            {
                ForecastResult = "No conflicts found.";
            }
        }

        [RelayCommand]
        private async Task RunConflictAnalysisAsync()
        {
            await CheckConflictsAsync();
        }

        [RelayCommand]
        private async Task SuggestScheduleAsync()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Please select a task first.");
                return;
            }

            var dependencies = await _context.TaskDependencies
                .Where(d => d.ParentTaskId == SelectedTask.Id || d.PrerequisiteTaskId == SelectedTask.Id)
                .ToListAsync();

            if (dependencies.Any())
            {
                ForecastResult = $"Task has {dependencies.Count} dependencies that need to be scheduled first.";
            }
            else
            {
                ForecastResult = "Task can be scheduled independently.";
            }
        }
    }
}
