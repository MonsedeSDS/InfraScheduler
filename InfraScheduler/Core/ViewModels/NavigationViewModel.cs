// ============================================================================
// NAMESPACE IMPORTS - These tell the compiler where to find the classes we use
// ============================================================================
using System.Collections.ObjectModel;      // For ObservableCollection<T> - automatically updates UI when items change
using System.Threading.Tasks;              // For async/await pattern - allows non-blocking operations
using System.Windows.Input;                // For ICommand interface - enables button clicks and user interactions
using Microsoft.Extensions.DependencyInjection;  // For DI container - manages object creation and dependencies
using CommunityToolkit.Mvvm.ComponentModel;      // For ObservableObject - base class for MVVM pattern
using CommunityToolkit.Mvvm.Input;               // For RelayCommand - converts methods to commands
using Microsoft.EntityFrameworkCore;            // For database operations
using InfraScheduler.Core.Views;           // For NavigationView, MainLandingView
using InfraScheduler.Inventory.Views;      // For EquipmentView, ToolView, etc.
using InfraScheduler.Delivery.Views;       // For JobView, JobTaskView, etc.
using InfraScheduler.Database.Views;       // For ClientView, SiteView, etc.
using InfraScheduler.Admin.Views;          // For UserView
using InfraScheduler.Views;                // For WorkflowDashboardView, etc.
using InfraScheduler.Data;                 // For InfraSchedulerContext
using InfraScheduler.Models;               // For Job, JobTask models
using InfraScheduler.Delivery.ViewModels;  // For JobViewModel, JobTaskViewModel
using InfraScheduler.Core.ViewModels;      // For MainLandingViewModel
using InfraScheduler.Inventory.ViewModels; // For InventoryLandingViewModel
using InfraScheduler.Database.ViewModels;  // For DatabaseLandingViewModel
using InfraScheduler.Delivery.ViewModels;  // For DeliveryLandingViewModel
using InfraScheduler.Admin.ViewModels;     // For UserViewModel

// ============================================================================
// NAMESPACE DECLARATION - Defines where this class lives in the project structure
// ============================================================================
namespace InfraScheduler.ViewModels
{
    // ============================================================================
    // MAIN CLASS - This is the central controller for navigation in the application
    // ============================================================================
    public partial class NavigationViewModel : ObservableObject  // 'partial' allows code generation, 'ObservableObject' enables UI updates
    {
        // ============================================================================
        // PRIVATE FIELDS - Internal data storage (not directly accessible from outside)
        // ============================================================================
        private readonly InfraSchedulerContext _context;        // Database context for data access
        private ObservableCollection<Job> _jobs = new();        // Collection of jobs that automatically updates UI
        private Job? _selectedJob;                              // Currently selected job (nullable)
        private JobTask? _selectedTask;                         // Currently selected task (nullable)
        private readonly JobViewModel _jobViewModel;            // ViewModel for job management
        private readonly JobTaskViewModel _jobTaskViewModel;    // ViewModel for task management
        private bool _isLoading;                                // Loading state indicator
        private readonly IServiceProvider _serviceProvider;     // Dependency injection container

        // ============================================================================
        // OBSERVABLE PROPERTY - Automatically generates property with UI update notifications
        // ============================================================================
        private object? _currentView;  // The view currently displayed in the main content area

        // ============================================================================
        // PUBLIC PROPERTIES - These are accessible from XAML and other classes
        // ============================================================================
        
        // Current View Property - Manages the currently displayed view with UI update notifications
        public object? CurrentView
        {
            get => _currentView;  // Return the private field
            set
            {
                if (_currentView != value)  // Only update if the value actually changed
                {
                    _currentView = value;  // Set the private field
                    OnPropertyChanged(nameof(CurrentView));  // Notify UI
                }
            }
        }
        
        // Jobs Collection Property - Provides access to the jobs list with UI update notifications
        public ObservableCollection<Job> Jobs
        {
            get => _jobs;  // Return the private field
            set
            {
                _jobs = value;  // Set the private field
                OnPropertyChanged(nameof(Jobs));  // Notify UI that this property changed
            }
        }

        // Selected Job Property - Manages the currently selected job with automatic data loading
        public Job? SelectedJob
        {
            get => _selectedJob;  // Return the private field
            set
            {
                // Only update if we have a different job selected
                if (_selectedJob is not null && value is not null && _selectedJob.Id != value.Id)
                {
                    _selectedJob = value;  // Set the private field
                    OnPropertyChanged(nameof(SelectedJob));  // Notify UI
                    // TODO: Implement job loading when JobViewModel.LoadJob method is available
                }
            }
        }

        // Selected Task Property - Manages the currently selected task with automatic data loading
        public JobTask? SelectedTask
        {
            get => _selectedTask;  // Return the private field
            set
            {
                // Only update if we have a different task selected
                if (_selectedTask is not null && value is not null && _selectedTask.Id != value.Id)
                {
                    _selectedTask = value;  // Set the private field
                    OnPropertyChanged(nameof(SelectedTask));  // Notify UI
                    // TODO: Implement task loading when JobTaskViewModel methods are available
                }
            }
        }

        // Loading State Property - Indicates when data is being loaded
        public bool IsLoading
        {
            get => _isLoading;  // Return the private field
            set
            {
                if (_isLoading != value)  // Only update if the value actually changed
                {
                    _isLoading = value;  // Set the private field
                    OnPropertyChanged(nameof(IsLoading));  // Notify UI
                }
            }
        }

        // Read-only Properties - Provide access to child ViewModels
        public JobViewModel JobViewModel => _jobViewModel;           // Expose job ViewModel
        public JobTaskViewModel JobTaskViewModel => _jobTaskViewModel;  // Expose task ViewModel

        // ============================================================================
        // COMMAND PROPERTIES - These enable button clicks and user interactions
        // ============================================================================
        
        // Data Loading Command - Triggers data refresh
        public ICommand LoadDataCommand { get; }
        
        // Landing Page Commands - Navigate to main section pages
        public ICommand ShowMainLandingCommand { get; }
        public ICommand ShowDeliveryLandingCommand { get; }
        public ICommand ShowInventoryLandingCommand { get; }
        public ICommand ShowDatabaseLandingCommand { get; }
        public ICommand ShowAdminLandingCommand { get; }
        public ICommand ShowAccountManagementLandingCommand { get; }
        public ICommand ShowSettingsLandingCommand { get; }
        public ICommand ShowHelpLandingCommand { get; }

        // Navigation Commands - Navigate to specific views
        public ICommand ShowClientViewCommand { get; }
        public ICommand ShowSubcontractorViewCommand { get; }
        public ICommand ShowSiteViewCommand { get; }
        public ICommand ShowTechnicianViewCommand { get; }
        public ICommand ShowJobViewCommand { get; }
        public ICommand ShowJobTaskViewCommand { get; }
        public ICommand ShowFinancialTransactionViewCommand { get; }
        public ICommand ShowUserViewCommand { get; }
        public ICommand ShowToolViewCommand { get; }
        public ICommand ShowEquipmentViewCommand { get; }
        public ICommand ShowWorkflowDashboardViewCommand { get; }
        public ICommand ShowEquipmentCategoryViewCommand { get; }
        public ICommand ShowJobAcceptanceViewCommand { get; }
        public ICommand ShowJobCloseOutViewCommand { get; }
        public ICommand ShowSiteEquipmentInventoryViewCommand { get; }
        public ICommand ShowProjectDetailViewCommand { get; }
        public ICommand ShowLogisticsShippingViewCommand { get; }
        public ICommand ShowReceivingViewCommand { get; }

        public NavigationViewModel(InfraSchedulerContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _jobViewModel = new JobViewModel(context);
            _jobTaskViewModel = new JobTaskViewModel(context);
            _serviceProvider = serviceProvider;

            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            
            // Start with Main Landing Page - properly set up with DataContext
            try
            {
                var mainLandingViewModel = new MainLandingViewModel(_serviceProvider, this);
                var mainLandingView = _serviceProvider.GetRequiredService<MainLandingView>();
                mainLandingView.DataContext = mainLandingViewModel;
                CurrentView = mainLandingView;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing MainLandingView: {ex.Message}");
                CurrentView = null;
            }

            // Initialize landing page commands
            ShowMainLandingCommand = CreateLandingCommand<MainLandingView, MainLandingViewModel>();
            ShowDeliveryLandingCommand = CreateLandingCommand<DeliveryLandingView, DeliveryLandingViewModel>();
            ShowInventoryLandingCommand = CreateLandingCommand<InventoryLandingView, InventoryLandingViewModel>();
            ShowDatabaseLandingCommand = CreateLandingCommand<DatabaseLandingView, DatabaseLandingViewModel>();

            // Placeholder commands for future implementation
            ShowAdminLandingCommand = new RelayCommand(() => { /* TODO */ });
            ShowAccountManagementLandingCommand = new RelayCommand(() => { /* TODO */ });
            ShowSettingsLandingCommand = new RelayCommand(() => { /* TODO */ });
            ShowHelpLandingCommand = new RelayCommand(() => { /* TODO */ });

            // Initialize navigation commands
            ShowClientViewCommand = CreateLandingCommand<ClientView, ClientViewModel>();
            ShowSubcontractorViewCommand = CreateLandingCommand<SubcontractorView, SubcontractorViewModel>();
            ShowSiteViewCommand = CreateLandingCommand<SiteView, SiteViewModel>();
            ShowTechnicianViewCommand = CreateLandingCommand<TechnicianView, TechnicianViewModel>();
            ShowJobViewCommand = CreateNavigationCommand<JobView, JobViewModel>();
            ShowJobTaskViewCommand = CreateNavigationCommand<JobTaskView, JobTaskViewModel>();
            ShowFinancialTransactionViewCommand = CreateNavigationCommand<FinancialTransactionView, FinancialTransactionViewModel>();
            ShowUserViewCommand = CreateNavigationCommand<UserView, UserViewModel>();
            ShowToolViewCommand = CreateLandingCommand<ToolView, ToolViewModel>();
            ShowEquipmentViewCommand = CreateLandingCommand<EquipmentView, EquipmentViewModel>();
            ShowWorkflowDashboardViewCommand = CreateNavigationCommand<WorkflowDashboardView, WorkflowDashboardViewModel>();
            ShowEquipmentCategoryViewCommand = CreateNavigationCommand<EquipmentCategoryView, EquipmentCategoryViewModel>();
            ShowJobAcceptanceViewCommand = CreateNavigationCommand<JobAcceptanceView, JobAcceptanceViewModel>();
            ShowJobCloseOutViewCommand = CreateNavigationCommand<JobCloseOutView, JobCloseOutViewModel>();
            ShowSiteEquipmentInventoryViewCommand = CreateNavigationCommand<SiteEquipmentInventoryView, SiteEquipmentInventoryViewModel>();
            ShowProjectDetailViewCommand = CreateNavigationCommand<ProjectDetailView, ProjectDetailViewModel>();
            ShowLogisticsShippingViewCommand = CreateNavigationCommand<LogisticsShippingView, LogisticsShippingViewModel>();
            ShowReceivingViewCommand = CreateNavigationCommand<ReceivingView, ReceivingViewModel>();
        }

        public NavigationViewModel() : this(App.Db, App.ServiceProvider) { }

        private ICommand CreateLandingCommand<TView, TViewModel>() 
            where TView : class 
            where TViewModel : class
        {
            return new AsyncRelayCommand(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"NavigationViewModel: CreateLandingCommand called for {typeof(TView).Name}");
                    
                    object viewModel;
                    
                    // Special case for MainLandingViewModel that requires NavigationViewModel instance
                    if (typeof(TViewModel) == typeof(MainLandingViewModel))
                    {
                        viewModel = new MainLandingViewModel(_serviceProvider, this);
                    }
                    // Special case for DatabaseLandingViewModel that requires NavigationViewModel instance
                    else if (typeof(TViewModel) == typeof(DatabaseLandingViewModel))
                    {
                        viewModel = new DatabaseLandingViewModel(_context, _serviceProvider, this);
                    }
                    // Special case for InventoryLandingViewModel that requires NavigationViewModel instance
                    else if (typeof(TViewModel) == typeof(InventoryLandingViewModel))
                    {
                        viewModel = new InventoryLandingViewModel(_context, _serviceProvider, this);
                    }
                    else
                    {
                        viewModel = _serviceProvider.GetRequiredService<TViewModel>();
                    }
                    
                    var view = _serviceProvider.GetRequiredService<TView>();
                    
                    // Set the DataContext for UserControl views
                    if (view is System.Windows.Controls.UserControl userControl)
                    {
                        userControl.DataContext = viewModel;
                    }
                    // Set the DataContext for Window views (fallback)
                    else if (view is System.Windows.Window window)
                    {
                        window.DataContext = viewModel;
                    }
                    
                    CurrentView = view;
                    System.Diagnostics.Debug.WriteLine($"NavigationViewModel: CurrentView set to {CurrentView?.GetType().Name}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"NavigationViewModel: Error in CreateLandingCommand: {ex.Message}");
                    // Fallback: create instances directly
                    try
                    {
                        object viewModel;
                        
                        // Special case for MainLandingViewModel
                        if (typeof(TViewModel) == typeof(MainLandingViewModel))
                        {
                            viewModel = new MainLandingViewModel(_serviceProvider, this);
                        }
                        // Special case for DatabaseLandingViewModel
                        else if (typeof(TViewModel) == typeof(DatabaseLandingViewModel))
                        {
                            viewModel = new DatabaseLandingViewModel(_context, _serviceProvider, this);
                        }
                        // Special case for InventoryLandingViewModel
                        else if (typeof(TViewModel) == typeof(InventoryLandingViewModel))
                        {
                            viewModel = new InventoryLandingViewModel(_context, _serviceProvider, this);
                        }
                        else
                        {
                            viewModel = Activator.CreateInstance<TViewModel>();
                        }
                        
                        var view = Activator.CreateInstance<TView>();
                        
                        // Set the DataContext for UserControl views
                        if (view is System.Windows.Controls.UserControl userControl)
                        {
                            userControl.DataContext = viewModel;
                        }
                        // Set the DataContext for Window views (fallback)
                        else if (view is System.Windows.Window window)
                        {
                            window.DataContext = viewModel;
                        }
                        
                        CurrentView = view;
                    }
                    catch (Exception fallbackEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error navigating to landing page {typeof(TView).Name}: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Fallback error: {fallbackEx.Message}");
                    }
                }
            });
        }

        private ICommand CreateNavigationCommand<TView, TViewModel>() 
            where TView : class 
            where TViewModel : class
        {
            return new AsyncRelayCommand(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"NavigationViewModel: CreateNavigationCommand called for {typeof(TView).Name}");
                    
                    var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
                    var view = _serviceProvider.GetRequiredService<TView>();
                    
                    // Set the DataContext for UserControl views
                    if (view is System.Windows.Controls.UserControl userControl)
                    {
                        userControl.DataContext = viewModel;
                    }
                    // Set the DataContext for Window views (fallback)
                    else if (view is System.Windows.Window window)
                    {
                        window.DataContext = viewModel;
                    }
                    
                    CurrentView = view;
                    System.Diagnostics.Debug.WriteLine($"NavigationViewModel: CurrentView set to {CurrentView?.GetType().Name}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"NavigationViewModel: Error in CreateNavigationCommand: {ex.Message}");
                    // Fallback: create instances directly
                    try
                    {
                        var viewModel = Activator.CreateInstance<TViewModel>();
                        var view = Activator.CreateInstance<TView>();
                        
                        // Set the DataContext for UserControl views
                        if (view is System.Windows.Controls.UserControl userControl)
                        {
                            userControl.DataContext = viewModel;
                        }
                        // Set the DataContext for Window views (fallback)
                        else if (view is System.Windows.Window window)
                        {
                            window.DataContext = viewModel;
                        }
                        
                        CurrentView = view;
                    }
                    catch (Exception fallbackEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error navigating to {typeof(TView).Name}: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Fallback error: {fallbackEx.Message}");
                    }
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