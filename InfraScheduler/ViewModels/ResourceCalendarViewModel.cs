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
    public partial class ResourceCalendarViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private int technicianId;
        [ObservableProperty] private DateTime date = DateTime.Now;
        [ObservableProperty] private bool isAvailable;
        [ObservableProperty] private string notes = string.Empty;
        [ObservableProperty] private ResourceCalendar? selectedResourceCalendar;

        public ObservableCollection<ResourceCalendar> ResourceCalendars { get; set; } = new();
        public ObservableCollection<Technician> Technicians { get; set; } = new();

        public ResourceCalendarViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");

            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _context = new InfraSchedulerContext(options);
            _context.Database.Migrate();

            LoadTechnicians();
            LoadResourceCalendars();
        }

        private void LoadTechnicians()
        {
            Technicians.Clear();
            foreach (var tech in _context.Technicians.ToList())
                Technicians.Add(tech);
        }

        private void LoadResourceCalendars()
        {
            ResourceCalendars.Clear();
            foreach (var rc in _context.ResourceCalendars
                .Include(r => r.Technician)
                .ToList())
                ResourceCalendars.Add(rc);
        }

        [RelayCommand]
        private void AddResourceCalendar()
        {
            if (TechnicianId <= 0)
            {
                MessageBox.Show("Please select a technician.");
                return;
            }

            var technician = _context.Technicians.Find(TechnicianId);
            if (technician == null)
            {
                MessageBox.Show("Selected technician not found.");
                return;
            }

            var newEntry = new ResourceCalendar
            {
                TechnicianId = TechnicianId,
                Date = Date,
                IsAvailable = IsAvailable,
                Notes = Notes
            };

            try
            {
                _context.ResourceCalendars.Add(newEntry);
                _context.SaveChanges();
                LoadResourceCalendars();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding resource calendar: {ex.Message}");
                _context.ChangeTracker.Clear();
            }
        }

        [RelayCommand]
        private void UpdateResourceCalendar()
        {
            if (SelectedResourceCalendar == null)
            {
                MessageBox.Show("Please select a resource calendar to update.");
                return;
            }

            if (TechnicianId <= 0)
            {
                MessageBox.Show("Please select a technician.");
                return;
            }

            var technician = _context.Technicians.Find(TechnicianId);
            if (technician == null)
            {
                MessageBox.Show("Selected technician not found.");
                return;
            }

            try
            {
                SelectedResourceCalendar.TechnicianId = TechnicianId;
                SelectedResourceCalendar.Date = Date;
                SelectedResourceCalendar.IsAvailable = IsAvailable;
                SelectedResourceCalendar.Notes = Notes;
                _context.SaveChanges();
                LoadResourceCalendars();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating resource calendar: {ex.Message}");
                _context.ChangeTracker.Clear();
            }
        }

        [RelayCommand]
        private void DeleteResourceCalendar()
        {
            if (SelectedResourceCalendar == null)
            {
                MessageBox.Show("Please select a resource calendar to delete.");
                return;
            }

            try
            {
                _context.ResourceCalendars.Remove(SelectedResourceCalendar);
                _context.SaveChanges();
                LoadResourceCalendars();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting resource calendar: {ex.Message}");
                _context.ChangeTracker.Clear();
            }
        }

        private void ClearFields()
        {
            TechnicianId = 0;
            Date = DateTime.Now;
            IsAvailable = true;
            Notes = string.Empty;
            SelectedResourceCalendar = null;
        }
    }
}
