using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Core.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class ProjectManagementViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _sectionTitle = "Project Management";

        [ObservableProperty]
        private string _sectionDescription = "Manage projects, track progress, and coordinate job deliveries across multiple client engagements.";

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Project> _projects = new();

        [ObservableProperty]
        private Project? _selectedProject;

        [ObservableProperty]
        private bool _isLoading;

        // KPI Properties
        [ObservableProperty]
        private int _totalProjects;

        [ObservableProperty]
        private int _activeProjects;

        [ObservableProperty]
        private int _completedProjects;

        [ObservableProperty]
        private double _overallCompletionPercentage;

        public ProjectManagementViewModel(InfraSchedulerContext context, IServiceProvider serviceProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceProvider = serviceProvider;
            
            Projects = new ObservableCollection<Project>();
            
            LoadProjectsAsync();
            LoadMetrics();
        }

        private async void LoadProjectsAsync()
        {
            try
            {
                IsLoading = true;
                var projects = await _context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Jobs)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                Projects.Clear();
                foreach (var project in projects)
                {
                    Projects.Add(project);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading projects: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadMetrics()
        {
            try
            {
                TotalProjects = _context.Projects?.Count() ?? 0;
                ActiveProjects = _context.Projects?.Count(p => p.Status == "Active") ?? 0;
                CompletedProjects = _context.Projects?.Count(p => p.Status == "Completed") ?? 0;
                
                if (TotalProjects > 0)
                {
                    OverallCompletionPercentage = (double)CompletedProjects / TotalProjects * 100;
                }
                else
                {
                    OverallCompletionPercentage = 0;
                }
            }
            catch (Exception ex)
            {
                // Handle database connection issues gracefully
                TotalProjects = 0;
                ActiveProjects = 0;
                CompletedProjects = 0;
                OverallCompletionPercentage = 0;
            }
        }

        [RelayCommand]
        private async Task CreateNewProject()
        {
            try
            {
                // TODO: Open Create Project dialog/view
                // For now, create a sample project
                var newProject = new Project
                {
                    Name = "New Project",
                    Description = "Project description",
                    ClientId = 1, // TODO: Allow user to select client
                    StartDate = DateTime.Today,
                    ProjectNumber = $"PRJ-{DateTime.Now:yyyyMMdd}-{TotalProjects + 1:000}",
                    Status = "Active"
                };

                _context.Projects.Add(newProject);
                await _context.SaveChangesAsync();
                
                LoadProjectsAsync();
                LoadMetrics();
                
                MessageBox.Show("New project created successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating project: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task OpenProject(Project? project)
        {
            if (project == null) return;

            try
            {
                SelectedProject = project;
                
                // Navigate to ProjectDetailView
                var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
                // TODO: Create and implement ShowProjectDetailViewCommand
                // navigationViewModel?.ShowProjectDetailViewCommand.Execute(project);
                
                MessageBox.Show($"Opening project: {project.Name}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening project: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SearchProjects()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadProjectsAsync();
                return;
            }

            try
            {
                IsLoading = true;
                var filteredProjects = await _context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Jobs)
                    .Where(p => p.Name.Contains(SearchText) || 
                               p.ProjectNumber.Contains(SearchText) ||
                               p.Client.Name.Contains(SearchText))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                Projects.Clear();
                foreach (var project in filteredProjects)
                {
                    Projects.Add(project);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching projects: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshProjects()
        {
            LoadProjectsAsync();
            LoadMetrics();
        }
    }
} 