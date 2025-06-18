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

namespace InfraScheduler.ViewModels
{
    public partial class AllocationViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private int jobTaskId;
        [ObservableProperty] private int technicianId;
        [ObservableProperty] private DateTime allocationDate = DateTime.Now;
        [ObservableProperty] private int hoursAllocated;
        [ObservableProperty] private Allocation? selectedAllocation;

        public ObservableCollection<Allocation> Allocations { get; set; } = new();
        public ObservableCollection<JobTask> JobTasks { get; set; } = new();
        public ObservableCollection<Technician> Technicians { get; set; } = new();

        public AllocationViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");

            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _context = new InfraSchedulerContext(options);
            _context.Database.Migrate();

            LoadJobTasks();
            LoadTechnicians();
            LoadAllocations();
        }

        private void LoadJobTasks()
        {
            JobTasks.Clear();
            foreach (var task in _context.JobTasks.ToList())
                JobTasks.Add(task);
        }

        private void LoadTechnicians()
        {
            Technicians.Clear();
            foreach (var tech in _context.Technicians.ToList())
                Technicians.Add(tech);
        }

        private void LoadAllocations()
        {
            Allocations.Clear();
            foreach (var alloc in _context.Allocations
                         .Include(a => a.JobTask)
                         .Include(a => a.Technician).ToList())
                Allocations.Add(alloc);
        }

        [RelayCommand]
        private void AddAllocation()
        {
            var newAllocation = new Allocation
            {
                JobTaskId = JobTaskId,
                TechnicianId = TechnicianId,
                AllocationDate = AllocationDate,
                HoursAllocated = HoursAllocated
            };
            _context.Allocations.Add(newAllocation);
            _context.SaveChanges();
            LoadAllocations();
            ClearFields();
        }

        [RelayCommand]
        private void UpdateAllocation()
        {
            if (SelectedAllocation == null) return;

            SelectedAllocation.JobTaskId = JobTaskId;
            SelectedAllocation.TechnicianId = TechnicianId;
            SelectedAllocation.AllocationDate = AllocationDate;
            SelectedAllocation.HoursAllocated = HoursAllocated;
            _context.SaveChanges();
            LoadAllocations();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteAllocation()
        {
            if (SelectedAllocation == null) return;

            _context.Allocations.Remove(SelectedAllocation);
            _context.SaveChanges();
            LoadAllocations();
            ClearFields();
        }

        private void ClearFields()
        {
            JobTaskId = 0;
            TechnicianId = 0;
            AllocationDate = DateTime.Now;
            HoursAllocated = 0;
            SelectedAllocation = null;
        }
    }
}
