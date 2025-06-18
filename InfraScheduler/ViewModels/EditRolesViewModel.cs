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
    public partial class EditRolesViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly Window _window;
        private readonly ObservableCollection<string> _originalRoles;

        [ObservableProperty]
        private ObservableCollection<string> selectedRoles;

        [ObservableProperty]
        private ObservableCollection<string> allRoles;

        [ObservableProperty]
        private string newRole = string.Empty;

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public EditRolesViewModel(InfraSchedulerContext context, Window window, ObservableCollection<string> roles)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _originalRoles = roles;
            SelectedRoles = new ObservableCollection<string>(roles);
            LoadAllRoles();
        }

        private async void LoadAllRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => r.Name)
                    .ToListAsync();
                
                AllRoles = new ObservableCollection<string>(roles);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading roles: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void RemoveRole(string role)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(role))
                {
                    MessageBox.Show("Please select a role to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to remove {role}?",
                    "Confirm Remove",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SelectedRoles.Remove(role);
                    AllRoles.Remove(role);
                    
                    // Remove from database
                    var roleToRemove = _context.Roles.FirstOrDefault(r => r.Name == role);
                    if (roleToRemove != null)
                    {
                        _context.Roles.Remove(roleToRemove);
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error removing role: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddNewRole()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewRole))
                {
                    MessageBox.Show("Please enter a role name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var trimmedRole = NewRole.Trim();
                if (trimmedRole.Length > 100)
                {
                    MessageBox.Show("Role name is too long (maximum 100 characters).", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedRoles.Contains(trimmedRole))
                {
                    MessageBox.Show("This role is already added.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.Roles.Add(new Role { Name = trimmedRole });
                await _context.SaveChangesAsync();

                // Refresh the list from database
                var roles = await _context.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => r.Name)
                    .ToListAsync();
                
                AllRoles = new ObservableCollection<string>(roles);
                NewRole = string.Empty;

                MessageBox.Show("New role added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding new role: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Save()
        {
            _originalRoles.Clear();
            foreach (var role in SelectedRoles)
            {
                _originalRoles.Add(role);
            }
            _window.DialogResult = true;
            _window.Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
} 