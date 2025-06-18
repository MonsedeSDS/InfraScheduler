using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class MaterialReservationViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private int materialResourceId;
        [ObservableProperty] private int jobTaskId;
        [ObservableProperty] private int quantity;
        [ObservableProperty] private DateTime reservedFrom;
        [ObservableProperty] private DateTime reservedTo;
        [ObservableProperty] private string status = "Pending";

        [ObservableProperty] private MaterialReservation? selectedReservation;

        public ObservableCollection<MaterialReservation> MaterialReservations { get; set; } = new();
        public ObservableCollection<MaterialResource> MaterialResources { get; set; } = new();
        public ObservableCollection<JobTask> JobTasks { get; set; } = new();

        public MaterialReservationViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");

            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _context = new InfraSchedulerContext(options);
            _context.Database.Migrate();

            LoadData();
        }

        private void LoadData()
        {
            MaterialResources = new ObservableCollection<MaterialResource>(_context.MaterialResources.ToList());
            JobTasks = new ObservableCollection<JobTask>(_context.JobTasks.ToList());
            LoadReservations();
        }

        private void LoadReservations()
        {
            MaterialReservations.Clear();
            foreach (var res in _context.MaterialReservations
                     .Include(m => m.MaterialResource)
                     .Include(m => m.JobTask))
            {
                MaterialReservations.Add(res);
            }
        }

        [RelayCommand]
        private void AddReservation()
        {
            var res = new MaterialReservation
            {
                MaterialResourceId = MaterialResourceId,
                JobTaskId = JobTaskId,
                Quantity = Quantity,
                ReservedFrom = ReservedFrom,
                ReservedTo = ReservedTo,
                Status = Status
            };

            _context.MaterialReservations.Add(res);
            _context.SaveChanges();
            LoadReservations();
            ClearFields();
        }

        [RelayCommand]
        private void UpdateReservation()
        {
            if (SelectedReservation == null) return;

            SelectedReservation.MaterialResourceId = MaterialResourceId;
            SelectedReservation.JobTaskId = JobTaskId;
            SelectedReservation.Quantity = Quantity;
            SelectedReservation.ReservedFrom = ReservedFrom;
            SelectedReservation.ReservedTo = ReservedTo;
            SelectedReservation.Status = Status;

            _context.SaveChanges();
            LoadReservations();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteReservation()
        {
            if (SelectedReservation == null) return;

            _context.MaterialReservations.Remove(SelectedReservation);
            _context.SaveChanges();
            LoadReservations();
            ClearFields();
        }

        private void ClearFields()
        {
            MaterialResourceId = 0;
            JobTaskId = 0;
            Quantity = 0;
            ReservedFrom = DateTime.Now;
            ReservedTo = DateTime.Now.AddDays(1);
            Status = "Pending";
            SelectedReservation = null;
        }
    }
}
