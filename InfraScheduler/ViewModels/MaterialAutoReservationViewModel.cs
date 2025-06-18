using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace InfraScheduler.ViewModels
{
    public partial class MaterialAutoReservationViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly MaterialAutoReservationService _service;

        public ObservableCollection<JobTask> JobTasks { get; set; } = new();
        public ObservableCollection<string> ReservationReport { get; set; } = new();

        [ObservableProperty]
        private JobTask? selectedJobTask;

        public MaterialAutoReservationViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");
            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}").Options;

            _context = new InfraSchedulerContext(options);
            _context.Database.Migrate();

            _service = new MaterialAutoReservationService(_context);
            LoadJobTasks();
        }

        private void LoadJobTasks()
        {
            JobTasks.Clear();
            foreach (var task in _context.JobTasks.ToList())
                JobTasks.Add(task);
        }

        [RelayCommand]
        private void RunMaterialReservation()
        {
            ReservationReport.Clear();

            if (SelectedJobTask == null)
            {
                ReservationReport.Add("❌ Please select a Job Task.");
                return;
            }

            foreach (var line in _service.ReserveMaterialsForJob(SelectedJobTask))
            {
                ReservationReport.Add(line);
            }
        }
    }
}
