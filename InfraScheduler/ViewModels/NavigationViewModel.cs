using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using InfraScheduler.Views;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using InfraScheduler.Models;
using InfraScheduler.Data;
using InfraScheduler.Services;

namespace InfraScheduler.ViewModels
{
    public partial class NavigationViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private ObservableCollection<Job> _jobs = new();
        private Job? _selectedJob;
        private JobTask? _selectedTask;
        private readonly JobViewModel _jobViewModel;
        private readonly JobTaskViewModel _jobTaskViewModel;
        private bool _isLoading;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private object? _currentView;

        public ObservableCollection<Job> Jobs
        {
            get => _jobs;
            set
            {
                _jobs = value;
                OnPropertyChanged(nameof(Jobs));
            }
        }

        public Job? SelectedJob
        {
            get => _selectedJob;
            set
            {
                if (_selectedJob is not null && value is not null && _selectedJob.Id != value.Id)
                {
                    _selectedJob = value;
                    OnPropertyChanged(nameof(SelectedJob));
                    _ = _jobViewModel.LoadJob(_selectedJob.Id);
                }
            }
        }

        public JobTask? SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask is not null && value is not null && _selectedTask.Id != value.Id)
                {
                    _selectedTask = value;
                    OnPropertyChanged(nameof(SelectedTask));
                    _jobTaskViewModel.SetJobId(_selectedTask.JobId);
                    _ = Task.Run(() => _jobTaskViewModel.LoadDataCommand.Execute(null));
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
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public JobViewModel JobViewModel => _jobViewModel;
        public JobTaskViewModel JobTaskViewModel => _jobTaskViewModel;

        public ICommand LoadDataCommand { get; }
        public ICommand ShowClientViewCommand { get; }
        public ICommand ShowSubcontractorViewCommand { get; }
        public ICommand ShowSiteViewCommand { get; }
        public ICommand ShowSiteTenantViewCommand { get; }
        public ICommand ShowSiteOwnerViewCommand { get; }
        public ICommand ShowTechnicianViewCommand { get; }
        public ICommand ShowTechnicianAssignmentViewCommand { get; }
        public ICommand ShowJobViewCommand { get; }
        public ICommand ShowJobTaskViewCommand { get; }
        public ICommand ShowTaskDependencyViewCommand { get; }
        public ICommand ShowFinancialTransactionViewCommand { get; }
        public ICommand ShowActivityLogViewCommand { get; }
        public ICommand ShowUserViewCommand { get; }
        public ICommand ShowToolViewCommand { get; }

        public NavigationViewModel(InfraSchedulerContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _jobViewModel = new JobViewModel(context);
            _jobTaskViewModel = new JobTaskViewModel(context);
            _serviceProvider = serviceProvider;

            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            CurrentView = new ClientView(new ClientViewModel(context));

            // Initialize navigation commands
            ShowClientViewCommand = CreateNavigationCommand<ClientView>();
            ShowSubcontractorViewCommand = CreateNavigationCommand<SubcontractorView>();
            ShowSiteViewCommand = CreateNavigationCommand<SiteView>();
            ShowSiteTenantViewCommand = CreateNavigationCommand<SiteTenantView>();
            ShowSiteOwnerViewCommand = CreateNavigationCommand<SiteOwnerView>();
            ShowTechnicianViewCommand = CreateNavigationCommand<TechnicianView>();
            ShowTechnicianAssignmentViewCommand = CreateNavigationCommand<TechnicianAssignmentView>();
            ShowJobViewCommand = CreateNavigationCommand<JobView>();
            ShowJobTaskViewCommand = CreateNavigationCommand<JobTaskView>();
            ShowTaskDependencyViewCommand = CreateNavigationCommand<TaskDependencyView>();
            ShowFinancialTransactionViewCommand = CreateNavigationCommand<FinancialTransactionView>();
            ShowActivityLogViewCommand = CreateNavigationCommand<ActivityLogView>();
            ShowUserViewCommand = CreateNavigationCommand<UserView>();
            ShowToolViewCommand = CreateNavigationCommand<ToolView>();
        }

        public NavigationViewModel() : this(App.Db, App.ServiceProvider) { }

        private ICommand CreateNavigationCommand<T>() where T : class
        {
            return new AsyncRelayCommand(async () =>
            {
                try
                {
                    CurrentView = _serviceProvider.GetRequiredService<T>();
                }
                catch (Exception ex)
                {
                    // TODO: Add proper error handling/logging
                    System.Diagnostics.Debug.WriteLine($"Error navigating to {typeof(T).Name}: {ex.Message}");
                }
            });
        }

        public async Task LoadDataAsync()
        {
            await LoadJobsAsync();
            await LoadTechniciansAsync();
        }

        private async Task LoadJobsAsync()
        {
            try
            {
                IsLoading = true;
                var jobs = await _context.Jobs
                    .Include(j => j.Tasks)
                    .ToListAsync();

                Jobs = new ObservableCollection<Job>(jobs);
            }
            catch (Exception ex)
            {
                // TODO: Add proper error handling/logging
                System.Diagnostics.Debug.WriteLine($"Error loading jobs: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTechniciansAsync()
        {
            try
            {
                IsLoading = true;
                var technicians = await _context.Technicians
                    //.Include(t => t.Allocations)
                    .ToListAsync();

                // Update any existing job tasks with technician information
                foreach (var job in Jobs)
                {
                    foreach (var task in job.Tasks)
                    {
                        if (task.TechnicianId.HasValue)
                        {
                            task.Technician = technicians.FirstOrDefault(t => t.Id == task.TechnicianId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: Add proper error handling/logging
                System.Diagnostics.Debug.WriteLine($"Error loading technicians: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
