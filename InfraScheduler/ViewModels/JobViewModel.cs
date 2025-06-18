using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using InfraScheduler.Commands;

namespace InfraScheduler.ViewModels
{
    public partial class JobViewModel : ObservableObject, INotifyPropertyChanged
    {
        private readonly InfraSchedulerContext _context;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _status = "Pending";
        private DateTime _startDate = DateTime.Today;
        private DateTime _endDate = DateTime.Today.AddDays(1);
        private ObservableCollection<JobTask> _tasks = new();
        private bool _isLoading;
        private ObservableCollection<Job> _jobs = new();
        private Job? _selectedJob;
        private bool _isInitialized;

        [ObservableProperty] private int siteId;
        [ObservableProperty] private int clientId;
        [ObservableProperty] private Job? selectedJob;

        public ObservableCollection<Job> Jobs
        {
            get => _jobs;
            set
            {
                _jobs = value;
                OnPropertyChanged(nameof(Jobs));
            }
        }

        public ObservableCollection<Site> Sites { get; set; } = new();
        public ObservableCollection<Client> Clients { get; set; } = new();

        public ICommand LoadDataCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand RemoveTaskCommand { get; }
        public ICommand UpdateTaskStatusCommand { get; }
        public ICommand ClearCommand { get; }

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

        public ObservableCollection<JobTask> Tasks
        {
            get => _tasks;
            set
            {
                _tasks = value;
                OnPropertyChanged(nameof(Tasks));
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

        public JobViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            
            LoadDataCommand = new RelayCommand(async () => await LoadData());
            SaveCommand = new RelayCommand(async () => await Save());
            DeleteCommand = new RelayCommand(async () => await Delete());
            AddTaskCommand = new RelayCommand(async () => await AddTask());
            RemoveTaskCommand = new RelayCommand(async () => await RemoveTask());
            UpdateTaskStatusCommand = new RelayCommand(async () => await UpdateTaskStatus());
            ClearCommand = new RelayCommand(async () => await Task.Run(ClearFields));

            // Initialize data loading
            InitializeAsync().ConfigureAwait(false);
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                IsLoading = true;
                await Task.WhenAll(
                    LoadJobs(),
                    LoadSites(),
                    LoadClients()
                );
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadJobs()
        {
            try
            {
                Jobs.Clear();
                var jobs = await _context.Jobs
                    .Include(j => j.Site)
                    .Include(j => j.Client)
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (var job in jobs)
                {
                    Jobs.Add(job);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading jobs: {ex.Message}");
            }
        }

        private async Task LoadSites()
        {
            try
            {
                Sites.Clear();
                var sites = await _context.Sites
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (var site in sites)
                {
                    Sites.Add(site);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sites: {ex.Message}");
            }
        }

        private async Task LoadClients()
        {
            try
            {
                Clients.Clear();
                var clients = await _context.Clients
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (var client in clients)
                {
                    Clients.Add(client);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading clients: {ex.Message}");
            }
        }

        private async Task LoadData()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
                return;
            }

            try
            {
                IsLoading = true;
                var jobs = await _context.Jobs
                    .Include(j => j.Tasks)
                    .OrderByDescending(j => j.CreatedAt)
                    .ToListAsync()
                    .ConfigureAwait(false);

                Jobs.Clear();
                foreach (var job in jobs)
                {
                    Jobs.Add(job);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading jobs: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadJob(int jobId)
        {
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Tasks)
                    .FirstOrDefaultAsync(j => j.Id == jobId)
                    .ConfigureAwait(false);

                if (job != null)
                {
                    SelectedJob = job;
                    Name = job.Name;
                    Description = job.Description;
                    Status = job.Status;
                    StartDate = job.StartDate;
                    EndDate = job.EndDate ?? DateTime.Today.AddDays(1);
                    SiteId = job.SiteId;
                    ClientId = job.ClientId;
                    await LoadTasks(jobId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading job: {ex.Message}");
            }
        }

        private async Task LoadTasks(int jobId)
        {
            try
            {
                var tasks = await _context.JobTasks
                    .Where(t => t.JobId == jobId)
                    .ToListAsync()
                    .ConfigureAwait(false);

                Tasks = new ObservableCollection<JobTask>(tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}");
            }
        }

        private async Task Save()
        {
            try
            {
                if (SelectedJob == null)
                {
                    SelectedJob = new Job
                    {
                        CreatedAt = DateTime.Now
                    };
                    _context.Jobs.Add(SelectedJob);
                }

                SelectedJob.Name = Name;
                SelectedJob.Description = Description;
                SelectedJob.Status = Status;
                SelectedJob.StartDate = StartDate;
                SelectedJob.EndDate = EndDate;
                SelectedJob.SiteId = SiteId;
                SelectedJob.ClientId = ClientId;

                await _context.SaveChangesAsync().ConfigureAwait(false);
                await LoadData();
                MessageBox.Show("Job saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving job: {ex.Message}");
            }
        }

        private async Task Delete()
        {
            try
            {
                if (SelectedJob == null)
                {
                    MessageBox.Show("Please select a job to delete.");
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to delete this job?", "Confirm Delete", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Jobs.Remove(SelectedJob);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    await LoadData();
                    ClearFields();
                    MessageBox.Show("Job deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting job: {ex.Message}");
            }
        }

        private async Task AddTask()
        {
            try
            {
                if (SelectedJob == null)
                {
                    MessageBox.Show("Please select a job first.");
                    return;
                }

                var newTask = new JobTask
                {
                    JobId = SelectedJob.Id,
                    Name = "New Task",
                    Status = "Pending",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1)
                };

                _context.JobTasks.Add(newTask);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                await LoadTasks(SelectedJob.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding task: {ex.Message}");
            }
        }

        private async Task RemoveTask()
        {
            try
            {
                if (SelectedJob == null)
                {
                    MessageBox.Show("Please select a job first.");
                    return;
                }

                var selectedTask = Tasks.FirstOrDefault(t => t.IsSelected);
                if (selectedTask == null)
                {
                    MessageBox.Show("Please select a task to remove.");
                    return;
                }

                _context.JobTasks.Remove(selectedTask);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                await LoadTasks(SelectedJob.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing task: {ex.Message}");
            }
        }

        private async Task UpdateTaskStatus()
        {
            try
            {
                if (SelectedJob == null)
                {
                    MessageBox.Show("Please select a job first.");
                    return;
                }

                var selectedTask = Tasks.FirstOrDefault(t => t.IsSelected);
                if (selectedTask == null)
                {
                    MessageBox.Show("Please select a task to update.");
                    return;
                }

                selectedTask.Status = "Completed";
                await _context.SaveChangesAsync().ConfigureAwait(false);
                await LoadTasks(SelectedJob.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating task status: {ex.Message}");
            }
        }

        private void ClearFields()
        {
            SelectedJob = null;
            Name = string.Empty;
            Description = string.Empty;
            Status = "Pending";
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1);
            SiteId = 0;
            ClientId = 0;
            Tasks.Clear();
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        public RelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
            }
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
