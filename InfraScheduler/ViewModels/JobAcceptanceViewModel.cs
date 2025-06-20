using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class JobAcceptanceViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly JobService _jobService;

        [ObservableProperty]
        private ObservableCollection<Job> availableJobs = new();

        [ObservableProperty]
        private Job? selectedJob;

        [ObservableProperty]
        private ObservableCollection<JobRequirement> jobRequirements = new();

        [ObservableProperty]
        private bool isLoading;

        public JobAcceptanceViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jobService = new JobService(_context);
            AvailableJobs = new ObservableCollection<Job>();
            JobRequirements = new ObservableCollection<JobRequirement>();
            
            LoadAvailableJobs();
        }

        private async void LoadAvailableJobs()
        {
            try
            {
                IsLoading = true;
                var jobs = await _context.Jobs
                    .Include(j => j.Site)
                    .Include(j => j.Client)
                    .Where(j => j.Status == "Pending" || j.Status == "Created")
                    .ToListAsync();

                AvailableJobs.Clear();
                foreach (var job in jobs)
                {
                    AvailableJobs.Add(job);
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
                LoadJobRequirements(value.Id);
            }
            else
            {
                JobRequirements.Clear();
            }
        }

        private async void LoadJobRequirements(int jobId)
        {
            try
            {
                var requirements = await _context.JobRequirements
                    .Include(r => r.EquipmentType)
                    .Where(r => r.JobId == jobId)
                    .ToListAsync();

                JobRequirements.Clear();
                foreach (var requirement in requirements)
                {
                    JobRequirements.Add(requirement);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading requirements: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AcceptJob()
        {
            if (SelectedJob == null)
            {
                MessageBox.Show("Please select a job to accept.");
                return;
            }

            try
            {
                IsLoading = true;
                var batch = await _jobService.AcceptJob(SelectedJob.Id);
                
                // Update job status
                SelectedJob.Status = "Accepted";
                await _context.SaveChangesAsync();

                MessageBox.Show($"Job accepted successfully! Equipment batch created with ID: {batch.Id}");
                
                // Refresh the jobs list
                LoadAvailableJobs();
                SelectedJob = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accepting job: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddRequirement()
        {
            if (SelectedJob == null)
            {
                MessageBox.Show("Please select a job first.");
                return;
            }

            // TODO: Open dialog to add requirement
            MessageBox.Show("Add requirement functionality will be implemented.");
        }

        [RelayCommand]
        private async Task EditRequirement()
        {
            // TODO: Open dialog to edit requirement
            MessageBox.Show("Edit requirement functionality will be implemented.");
        }

        [RelayCommand]
        private async Task RemoveRequirement()
        {
            // TODO: Remove selected requirement
            MessageBox.Show("Remove requirement functionality will be implemented.");
        }
    }
} 