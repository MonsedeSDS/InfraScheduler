using InfraScheduler.Commands;
using InfraScheduler.Data;
using InfraScheduler.Services;
using InfraScheduler.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public class WorkflowDashboardViewModel : ViewModelBase
    {
        private readonly InfraSchedulerContext _context;
        private readonly WorkflowOrchestrator _workflowOrchestrator;
        private WorkflowStatus? _selectedWorkflowStatus;

        public WorkflowDashboardViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _workflowOrchestrator = new WorkflowOrchestrator(context);
            
            WorkflowStatuses = new ObservableCollection<WorkflowStatus>();
            
            InitializeCommands();
            _ = LoadWorkflowStatuses();
        }

        public ObservableCollection<WorkflowStatus> WorkflowStatuses { get; set; }

        public WorkflowStatus? SelectedWorkflowStatus
        {
            get => _selectedWorkflowStatus;
            set
            {
                _selectedWorkflowStatus = value;
                OnPropertyChanged(nameof(SelectedWorkflowStatus));
            }
        }

        public RelayCommand RefreshCommand { get; private set; } = null!;
        public RelayCommand AcceptJobCommand { get; private set; } = null!;
        public RelayCommand OpenReceivingViewCommand { get; private set; } = null!;
        public RelayCommand OpenShippingViewCommand { get; private set; } = null!;
        public RelayCommand ViewTasksCommand { get; private set; } = null!;
        public RelayCommand OpenCloseOutViewCommand { get; private set; } = null!;
        public RelayCommand ViewSiteInventoryCommand { get; private set; } = null!;
        public RelayCommand RebuildSnapshotsCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            RefreshCommand = new RelayCommand(async () => await LoadWorkflowStatuses());
            AcceptJobCommand = new RelayCommand(async () => await AcceptJob());
            OpenReceivingViewCommand = new RelayCommand(async () => await OpenReceivingView());
            OpenShippingViewCommand = new RelayCommand(async () => await OpenShippingView());
            ViewTasksCommand = new RelayCommand(async () => await ViewTasks());
            OpenCloseOutViewCommand = new RelayCommand(async () => await OpenCloseOutView());
            ViewSiteInventoryCommand = new RelayCommand(async () => await ViewSiteInventory());
            RebuildSnapshotsCommand = new RelayCommand(async () => await RebuildSnapshots());
        }

        private async Task LoadWorkflowStatuses()
        {
            try
            {
                WorkflowStatuses.Clear();

                // Get all jobs with equipment requirements
                var jobsWithRequirements = await _context.Jobs
                    .Include(j => j.Requirements)
                    .Include(j => j.Site)
                    .Include(j => j.Client)
                    .Where(j => j.Requirements.Any())
                    .ToListAsync();

                foreach (var job in jobsWithRequirements)
                {
                    var status = await _workflowOrchestrator.GetWorkflowStatus(job.Id);
                    WorkflowStatuses.Add(status);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading workflow statuses: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AcceptJob()
        {
            if (SelectedWorkflowStatus == null)
            {
                MessageBox.Show("Please select a job to accept.", "No Job Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var batch = await _workflowOrchestrator.AcceptJob(SelectedWorkflowStatus.JobId);
                
                MessageBox.Show($"Job '{SelectedWorkflowStatus.JobName}' accepted successfully. Equipment batch created with ID: {batch.Id}", 
                    "Job Accepted", MessageBoxButton.OK, MessageBoxImage.Information);
                
                await LoadWorkflowStatuses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accepting job: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OpenReceivingView()
        {
            if (SelectedWorkflowStatus == null)
            {
                MessageBox.Show("Please select a job first.", "No Job Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var receivingView = new ReceivingView();
                var receivingViewModel = new ReceivingViewModel(_context);
                receivingView.DataContext = receivingViewModel;
                
                var window = new Window
                {
                    Title = $"Receiving Equipment - {SelectedWorkflowStatus.JobName}",
                    Content = receivingView,
                    Width = 1000,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening receiving view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OpenShippingView()
        {
            if (SelectedWorkflowStatus == null)
            {
                MessageBox.Show("Please select a job first.", "No Job Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var shippingView = new LogisticsShippingView();
                var shippingViewModel = new LogisticsShippingViewModel(_context);
                shippingView.DataContext = shippingViewModel;
                
                var window = new Window
                {
                    Title = $"Shipping Equipment - {SelectedWorkflowStatus.JobName}",
                    Content = shippingView,
                    Width = 1000,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening shipping view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ViewTasks()
        {
            if (SelectedWorkflowStatus == null)
            {
                MessageBox.Show("Please select a job first.", "No Job Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var jobTaskViewModel = new JobTaskViewModel(_context);
                var jobTaskView = new JobTaskView(jobTaskViewModel);
                
                var window = new Window
                {
                    Title = $"Job Tasks - {SelectedWorkflowStatus.JobName}",
                    Content = jobTaskView,
                    Width = 1000,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening tasks view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OpenCloseOutView()
        {
            if (SelectedWorkflowStatus == null)
            {
                MessageBox.Show("Please select a job first.", "No Job Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var closeOutView = new JobCloseOutView();
                var closeOutViewModel = new JobCloseOutViewModel(_context);
                closeOutView.DataContext = closeOutViewModel;
                
                var window = new Window
                {
                    Title = $"Job Close-Out - {SelectedWorkflowStatus.JobName}",
                    Content = closeOutView,
                    Width = 1000,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening close-out view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ViewSiteInventory()
        {
            if (SelectedWorkflowStatus == null)
            {
                MessageBox.Show("Please select a job first.", "No Job Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var inventoryView = new SiteEquipmentInventoryView();
                var inventoryViewModel = new SiteEquipmentInventoryViewModel(_context);
                inventoryView.DataContext = inventoryViewModel;
                
                var window = new Window
                {
                    Title = $"Site Equipment Inventory - {SelectedWorkflowStatus.SiteName}",
                    Content = inventoryView,
                    Width = 1000,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening site inventory view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RebuildSnapshots()
        {
            try
            {
                await _workflowOrchestrator.RebuildSiteEquipmentSnapshots();
                MessageBox.Show("Site equipment snapshots rebuilt successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rebuilding snapshots: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 