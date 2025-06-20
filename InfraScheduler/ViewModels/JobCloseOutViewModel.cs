using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Models.EquipmentManagement;
using InfraScheduler.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class JobCloseOutViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly JobService _jobService;
        private readonly SiteEquipmentService _siteEquipmentService;

        [ObservableProperty]
        private ObservableCollection<Job> readyToCloseJobs = new();

        [ObservableProperty]
        private Job? selectedJob;

        [ObservableProperty]
        private ObservableCollection<EquipmentLine> equipmentLines = new();

        [ObservableProperty]
        private bool isLoading;

        public JobCloseOutViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jobService = new JobService(_context);
            _siteEquipmentService = new SiteEquipmentService(_context);
            
            ReadyToCloseJobs = new ObservableCollection<Job>();
            EquipmentLines = new ObservableCollection<EquipmentLine>();
            
            LoadReadyToCloseJobs();
        }

        private async void LoadReadyToCloseJobs()
        {
            try
            {
                IsLoading = true;
                var jobs = await _context.Jobs
                    .Include(j => j.Site)
                    .Include(j => j.Client)
                    .Where(j => j.Status == "Accepted" || j.Status == "In Progress")
                    .ToListAsync();

                ReadyToCloseJobs.Clear();
                foreach (var job in jobs)
                {
                    ReadyToCloseJobs.Add(job);
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

        partial void OnSelectedJobChanged(Job? value)
        {
            if (value != null)
            {
                LoadEquipmentLines(value.Id);
            }
            else
            {
                EquipmentLines.Clear();
            }
        }

        private async void LoadEquipmentLines(int jobId)
        {
            try
            {
                var batch = await _context.EquipmentBatches
                    .Include(b => b.Lines)
                    .ThenInclude(l => l.EquipmentType)
                    .FirstOrDefaultAsync(b => b.JobId == jobId);

                if (batch != null)
                {
                    EquipmentLines.Clear();
                    foreach (var line in batch.Lines)
                    {
                        EquipmentLines.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading equipment lines: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task CloseJob()
        {
            if (SelectedJob == null)
            {
                MessageBox.Show("Please select a job to close.");
                return;
            }

            // Check if all equipment lines are installed
            var uninstalledLines = EquipmentLines.Where(l => l.Status != EquipmentStatus.OnSiteInstalled);
            if (uninstalledLines.Any())
            {
                MessageBox.Show($"Cannot close job: {uninstalledLines.Count()} equipment lines are not installed.");
                return;
            }

            try
            {
                IsLoading = true;
                await _jobService.CloseJob(SelectedJob.Id);
                
                MessageBox.Show("Job closed successfully! Equipment deployment report generated.");
                
                // Refresh the jobs list
                LoadReadyToCloseJobs();
                SelectedJob = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing job: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GenerateReport()
        {
            if (SelectedJob == null)
            {
                MessageBox.Show("Please select a job first.");
                return;
            }

            try
            {
                // TODO: Implement Excel report generation
                MessageBox.Show("Excel report generation will be implemented later.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ViewInstallationDetails()
        {
            if (SelectedJob == null)
            {
                MessageBox.Show("Please select a job first.");
                return;
            }

            try
            {
                var installedLines = EquipmentLines.Where(l => l.Status == EquipmentStatus.OnSiteInstalled);
                var message = $"Installation Details for {SelectedJob.Name}:\n\n";
                
                foreach (var line in installedLines)
                {
                    message += $"Equipment: {line.EquipmentType.Name}\n";
                    message += $"Quantity: {line.ReceivedQty}\n";
                    message += $"Installed: {line.InstalledDate:yyyy-MM-dd HH:mm}\n\n";
                }

                MessageBox.Show(message, "Installation Details");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing installation details: {ex.Message}");
            }
        }
    }
} 