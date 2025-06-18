using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;

namespace InfraScheduler.ViewModels
{
    public partial class TechnicianAssignmentViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private int jobTaskId;
        [ObservableProperty] private int technicianId;
        [ObservableProperty] private TechnicianAssignment? selectedAssignment;

        public ObservableCollection<TechnicianAssignment> TechnicianAssignments { get; set; } = new();
        public ObservableCollection<Technician> Technicians { get; set; } = new();
        public ObservableCollection<JobTask> JobTasks { get; set; } = new();

        public TechnicianAssignmentViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");

            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _context = new InfraSchedulerContext(options);
            _context.Database.Migrate();

            LoadTechnicians();
            LoadJobTasks();
            LoadAssignments();
        }

        private void LoadTechnicians()
        {
            Technicians.Clear();
            foreach (var t in _context.Technicians.ToList())
                Technicians.Add(t);
        }

        private void LoadJobTasks()
        {
            JobTasks.Clear();
            foreach (var jt in _context.JobTasks.ToList())
                JobTasks.Add(jt);
        }

        private void LoadAssignments()
        {
            TechnicianAssignments.Clear();
            foreach (var a in _context.TechnicianAssignments.Include(a => a.JobTask).Include(a => a.Technician).ToList())
                TechnicianAssignments.Add(a);
        }

        [RelayCommand]
        private void AddAssignment()
        {
            var newAssignment = new TechnicianAssignment
            {
                TechnicianId = TechnicianId,
                JobTaskId = JobTaskId
            };
            _context.TechnicianAssignments.Add(newAssignment);
            _context.SaveChanges();
            LoadAssignments();
            ClearFields();
        }

        [RelayCommand]
        private void UpdateAssignment()
        {
            if (SelectedAssignment == null) return;

            SelectedAssignment.TechnicianId = TechnicianId;
            SelectedAssignment.JobTaskId = JobTaskId;
            _context.SaveChanges();
            LoadAssignments();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteAssignment()
        {
            if (SelectedAssignment == null) return;

            _context.TechnicianAssignments.Remove(SelectedAssignment);
            _context.SaveChanges();
            LoadAssignments();
            ClearFields();
        }

        private void ClearFields()
        {
            TechnicianId = 0;
            JobTaskId = 0;
            SelectedAssignment = null;
        }
    }
}
