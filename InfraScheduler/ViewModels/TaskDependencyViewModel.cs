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
    public partial class TaskDependencyViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private int parentTaskId;
        [ObservableProperty] private int prerequisiteTaskId;
        [ObservableProperty] private TaskDependency? selectedDependency;

        public ObservableCollection<TaskDependency> TaskDependencies { get; set; } = new();
        public ObservableCollection<JobTask> JobTasks { get; set; } = new();

        public TaskDependencyViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");

            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _context = new InfraSchedulerContext(options);
            _context.Database.Migrate();

            LoadJobTasks();
            LoadDependencies();
        }

        private void LoadJobTasks()
        {
            JobTasks.Clear();
            foreach (var task in _context.JobTasks.ToList())
                JobTasks.Add(task);
        }

        private void LoadDependencies()
        {
            TaskDependencies.Clear();
            foreach (var dep in _context.TaskDependencies
                .Include(td => td.ParentTask)
                .Include(td => td.PrerequisiteTask)
                .ToList())
                TaskDependencies.Add(dep);
        }

        [RelayCommand]
        private void AddDependency()
        {
            if (ParentTaskId == PrerequisiteTaskId)
            {
                MessageBox.Show("A task cannot depend on itself.");
                return;
            }

            var newDep = new TaskDependency
            {
                ParentTaskId = ParentTaskId,
                PrerequisiteTaskId = PrerequisiteTaskId
            };

            _context.TaskDependencies.Add(newDep);
            _context.SaveChanges();
            LoadDependencies();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteDependency()
        {
            if (SelectedDependency == null)
            {
                MessageBox.Show("Select a dependency to delete.");
                return;
            }

            _context.TaskDependencies.Remove(SelectedDependency);
            _context.SaveChanges();
            LoadDependencies();
            ClearFields();
        }

        private void ClearFields()
        {
            ParentTaskId = 0;
            PrerequisiteTaskId = 0;
            SelectedDependency = null;
        }
    }
}
