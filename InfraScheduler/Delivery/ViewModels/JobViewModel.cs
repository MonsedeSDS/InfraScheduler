using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace InfraScheduler.Delivery.ViewModels
{
    public partial class JobViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _description = string.Empty;
        [ObservableProperty] private DateTime? _startDate;
        [ObservableProperty] private DateTime? _endDate;
        [ObservableProperty] private string _status = "Planning";
        [ObservableProperty] private int? _siteId;
        [ObservableProperty] private int? _clientId;
        [ObservableProperty] private Job? _selectedJob;

        [ObservableProperty] private ObservableCollection<Job> _jobs = new();
        [ObservableProperty] private ObservableCollection<Site> _sites = new();
        [ObservableProperty] private ObservableCollection<Client> _clients = new();

        public JobViewModel(InfraSchedulerContext context)
        {
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            var jobs = _context.Jobs
                .Include(j => j.Site)
                .Include(j => j.Client)
                .ToList();

            Jobs.Clear();
            foreach (var job in jobs)
            {
                Jobs.Add(job);
            }

            var sites = _context.Sites.ToList();
            Sites.Clear();
            foreach (var site in sites)
            {
                Sites.Add(site);
            }

            var clients = _context.Clients.ToList();
            Clients.Clear();
            foreach (var client in clients)
            {
                Clients.Add(client);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                if (SelectedJob == null)
                {
                    // Create new job
                    var job = new Job
                    {
                        Name = Name,
                        Description = Description,
                        StartDate = StartDate ?? DateTime.Today,
                        EndDate = EndDate,
                        Status = Status,
                        SiteId = SiteId ?? 0,
                        ClientId = ClientId ?? 0
                    };

                    _context.Jobs.Add(job);
                }
                else
                {
                    // Update existing job
                    SelectedJob.Name = Name;
                    SelectedJob.Description = Description;
                    SelectedJob.StartDate = StartDate ?? DateTime.Today;
                    SelectedJob.EndDate = EndDate;
                    SelectedJob.Status = Status;
                    SelectedJob.SiteId = SiteId ?? 0;
                    SelectedJob.ClientId = ClientId ?? 0;
                }

                await _context.SaveChangesAsync();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error saving job: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedJob == null) return;

            try
            {
                _context.Jobs.Remove(SelectedJob);
                await _context.SaveChangesAsync();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error deleting job: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Description = string.Empty;
            StartDate = null;
            EndDate = null;
            Status = "Planning";
            SiteId = null;
            ClientId = null;
            SelectedJob = null;
        }

        partial void OnSelectedJobChanged(Job? value)
        {
            if (value != null)
            {
                Name = value.Name;
                Description = value.Description;
                StartDate = value.StartDate;
                EndDate = value.EndDate;
                Status = value.Status;
                SiteId = value.SiteId;
                ClientId = value.ClientId;
            }
        }
    }
} 