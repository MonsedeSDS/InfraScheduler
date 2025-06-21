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
    public partial class ProjectDetailViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _description = string.Empty;
        [ObservableProperty] private DateTime? _startDate;
        [ObservableProperty] private DateTime? _endDate;
        [ObservableProperty] private string _status = "Planning";
        [ObservableProperty] private Project? _selectedProject;

        [ObservableProperty] private ObservableCollection<Project> _projects = new();

        public ProjectDetailViewModel(InfraSchedulerContext context)
        {
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            var projects = _context.Projects.ToList();

            Projects.Clear();
            foreach (var project in projects)
            {
                Projects.Add(project);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                if (SelectedProject == null)
                {
                    // Create new project
                    var project = new Project
                    {
                        Name = Name,
                        Description = Description,
                        StartDate = StartDate ?? DateTime.Today,
                        EndDate = EndDate,
                        Status = Status,
                        // WorkflowProgress property doesn't exist - removing
                        CreatedAt = DateTime.Now
                    };

                    _context.Projects.Add(project);
                }
                else
                {
                    // Update existing project
                    SelectedProject.Name = Name;
                    SelectedProject.Description = Description;
                    SelectedProject.StartDate = StartDate ?? DateTime.Today;
                    SelectedProject.EndDate = EndDate;
                    SelectedProject.Status = Status;
                }

                await _context.SaveChangesAsync();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error saving project: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedProject == null) return;

            try
            {
                _context.Projects.Remove(SelectedProject);
                await _context.SaveChangesAsync();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error deleting project: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Description = string.Empty;
            StartDate = null;
            EndDate = null;
            Status = "Planning";
            SelectedProject = null;
        }

        partial void OnSelectedProjectChanged(Project? value)
        {
            if (value != null)
            {
                Name = value.Name;
                Description = value.Description;
                StartDate = value.StartDate;
                EndDate = value.EndDate;
                Status = value.Status;
            }
        }
    }
} 