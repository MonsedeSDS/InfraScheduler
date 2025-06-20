using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace InfraScheduler.ViewModels
{
    public partial class TechnicianManagementLandingViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _sectionTitle = "Technician Management";

        [ObservableProperty]
        private string _sectionDescription = "Manage technicians, assignments, and team coordination. Track certifications and skills.";

        [ObservableProperty]
        private int _totalTechnicians;

        [ObservableProperty]
        private int _availableTechnicians;

        [ObservableProperty]
        private int _assignedTechnicians;

        [ObservableProperty]
        private int _totalCertifications;

        [ObservableProperty]
        private int _totalAssignments;

        public ObservableCollection<QuickActionCard> QuickActions { get; set; }

        public TechnicianManagementLandingViewModel(InfraSchedulerContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            QuickActions = new ObservableCollection<QuickActionCard>();
            LoadQuickActions();
            LoadMetrics();
        }

        private void LoadQuickActions()
        {
            QuickActions.Clear();
            
            QuickActions.Add(new QuickActionCard
            {
                Title = "Technicians",
                Description = "View and manage all technicians",
                Icon = "👷",
                Command = new RelayCommand(() => NavigateToTechnicians())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Technician Assignments",
                Description = "Manage technician job assignments",
                Icon = "📋",
                Command = new RelayCommand(() => NavigateToTechnicianAssignments())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Certifications",
                Description = "Manage technician certifications",
                Icon = "🎓",
                Command = new RelayCommand(() => NavigateToCertifications())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Skills Management",
                Description = "Track technician skills and competencies",
                Icon = "⚡",
                Command = new RelayCommand(() => NavigateToSkills())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Team Overview",
                Description = "View team performance and availability",
                Icon = "👥",
                Command = new RelayCommand(() => NavigateToTeamOverview())
            });

            QuickActions.Add(new QuickActionCard
            {
                Title = "Availability Tracking",
                Description = "Monitor technician availability and schedules",
                Icon = "📅",
                Command = new RelayCommand(() => NavigateToAvailability())
            });
        }

        private void LoadMetrics()
        {
            try
            {
                TotalTechnicians = _context.Technicians?.Count() ?? 0;
                AvailableTechnicians = 0; // Technician model doesn't have Status property
                AssignedTechnicians = 0; // Technician model doesn't have Status property
                TotalCertifications = _context.Certifications?.Count() ?? 0;
                TotalAssignments = _context.TechnicianAssignments?.Count() ?? 0;
            }
            catch (Exception ex)
            {
                // Handle database connection issues gracefully
                TotalTechnicians = 0;
                AvailableTechnicians = 0;
                AssignedTechnicians = 0;
                TotalCertifications = 0;
                TotalAssignments = 0;
            }
        }

        [RelayCommand]
        private async Task NavigateToTechnicians()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowTechnicianViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToTechnicianAssignments()
        {
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            navigationViewModel?.ShowTechnicianAssignmentViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToCertifications()
        {
            // This would need to be implemented when CertificationsView is created
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            // navigationViewModel?.ShowCertificationsViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToSkills()
        {
            // This would need to be implemented when SkillsView is created
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            // navigationViewModel?.ShowSkillsViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToTeamOverview()
        {
            // This would need to be implemented when TeamOverviewView is created
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            // navigationViewModel?.ShowTeamOverviewViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task NavigateToAvailability()
        {
            // This would need to be implemented when AvailabilityView is created
            var navigationViewModel = _serviceProvider.GetService<NavigationViewModel>();
            // navigationViewModel?.ShowAvailabilityViewCommand.Execute(null);
        }

        [RelayCommand]
        private async Task RefreshMetrics()
        {
            LoadMetrics();
        }
    }
} 