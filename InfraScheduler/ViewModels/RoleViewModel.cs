using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class RoleViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty]
        private ObservableCollection<Role> roles;

        [ObservableProperty]
        private Role? selectedRole;

        [ObservableProperty]
        private string searchTerm = string.Empty;

        [ObservableProperty]
        private string newRoleName = string.Empty;

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public RoleViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Roles = new ObservableCollection<Role>();
            LoadRoles();
        }

        private async void LoadRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                Roles.Clear();
                foreach (var role in roles)
                {
                    Roles.Add(role);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading roles: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddRole()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewRoleName))
                {
                    MessageBox.Show("Please enter a role name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Roles.Any(r => r.Name.Equals(NewRoleName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A role with this name already exists.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newRole = new Role
                {
                    Name = NewRoleName.Trim()
                };

                _context.Roles.Add(newRole);
                await _context.SaveChangesAsync();

                Roles.Add(newRole);
                NewRoleName = string.Empty;

                MessageBox.Show("Role added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding role: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteRole()
        {
            try
            {
                if (SelectedRole == null)
                {
                    MessageBox.Show("Please select a role to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete the role '{SelectedRole.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Roles.Remove(SelectedRole);
                    await _context.SaveChangesAsync();
                    Roles.Remove(SelectedRole);
                    SelectedRole = null;

                    MessageBox.Show("Role deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting role: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ClearSelection()
        {
            SelectedRole = null;
            NewRoleName = string.Empty;
        }
    }
} 