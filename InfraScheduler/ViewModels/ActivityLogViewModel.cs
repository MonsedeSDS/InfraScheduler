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
    public partial class ActivityLogViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private DateTime timestamp = DateTime.Now;
        [ObservableProperty] private string action = string.Empty;
        [ObservableProperty] private string entityAffected = string.Empty;
        [ObservableProperty] private int userId;
        [ObservableProperty] private ActivityLog? selectedLog;

        public ObservableCollection<ActivityLog> ActivityLogs { get; set; } = new();

        public ActivityLogViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");

            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _context = new InfraSchedulerContext(options);

            // THE MISSING PART:
            _context.Database.Migrate();

            LoadLogs();
        }


        private void LoadLogs()
        {
            ActivityLogs.Clear();
            foreach (var log in _context.ActivityLogs.ToList())
            {
                ActivityLogs.Add(log);
            }
        }

        [RelayCommand]
        private void AddLog()
        {
            var newLog = new ActivityLog
            {
                CreatedAt = Timestamp, // Fixed property name
                Action = Action,
                EntityAffected = EntityAffected,
                UserId = UserId
            };

            _context.ActivityLogs.Add(newLog);
            _context.SaveChanges();
            LoadLogs();
            ClearFields();
        }

        [RelayCommand]
        private void UpdateLog()
        {
            if (SelectedLog == null)
            {
                MessageBox.Show("Please select a log to update.");
                return;
            }

            SelectedLog.CreatedAt = Timestamp; // Fixed property name
            SelectedLog.Action = Action;
            SelectedLog.EntityAffected = EntityAffected;
            SelectedLog.UserId = UserId;

            _context.SaveChanges();
            LoadLogs();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteLog()
        {
            if (SelectedLog == null)
            {
                MessageBox.Show("Please select a log to delete.");
                return;
            }

            _context.ActivityLogs.Remove(SelectedLog);
            _context.SaveChanges();
            LoadLogs();
            ClearFields();
        }

        private void ClearFields()
        {
            Timestamp = DateTime.Now;
            Action = string.Empty;
            EntityAffected = string.Empty;
            UserId = 0;
            SelectedLog = null;
        }
    }
}
