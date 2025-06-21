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
using System.Timers;

namespace InfraScheduler.Admin.ViewModels
{
    public partial class UserViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly System.Timers.Timer _searchTimer;
        private ObservableCollection<User> _allUsers;

        [ObservableProperty]
        private ObservableCollection<User> users;

        [ObservableProperty]
        private string searchTerm = string.Empty;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string fullName = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string role = "User";

        [ObservableProperty]
        private string passwordHash = string.Empty;

        [ObservableProperty]
        private User? selectedUser;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public UserViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _allUsers = new ObservableCollection<User>();
            Users = new ObservableCollection<User>();

            // Initialize search timer
            _searchTimer = new System.Timers.Timer(300); // 300ms delay
            _searchTimer.Elapsed += (s, e) => 
            {
                _searchTimer.Stop();
                Application.Current.Dispatcher.Invoke(() => FilterUsers());
            };

            LoadUsers();
        }

        private async Task LoadUsers()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _allUsers.Clear();
                var users = await _context.Users.ToListAsync();
                foreach (var user in users)
                {
                    _allUsers.Add(user);
                }
                FilterUsers();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading users: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSearchTermChanged(string value)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void FilterUsers()
        {
            Users.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchTerm) 
                ? _allUsers 
                : _allUsers.Where(u => 
                    (u.Username?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.FullName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.Email?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

            foreach (var user in filtered)
            {
                Users.Add(user);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateUserData()) return;

            try
            {
                var user = new User
                {
                    Username = Username,
                    FullName = FullName,
                    Email = Email,
                    Role = Role,
                    PasswordHash = GeneratePasswordHash(PasswordHash), // In real app, hash the password
                    // IsActive property doesn't exist - removing
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                await LoadUsers();
                ClearFields();
                MessageBox.Show("User added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving user: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Update()
        {
            if (SelectedUser == null || !ValidateUserData()) return;

            try
            {
                SelectedUser.Username = Username;
                SelectedUser.FullName = FullName;
                SelectedUser.Email = Email;
                SelectedUser.Role = Role;
                
                if (!string.IsNullOrWhiteSpace(PasswordHash))
                {
                    SelectedUser.PasswordHash = GeneratePasswordHash(PasswordHash);
                }

                await _context.SaveChangesAsync();
                await LoadUsers();
                ClearFields();
                MessageBox.Show("User updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating user: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete the user '{SelectedUser.Username}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Users.Remove(SelectedUser);
                    await _context.SaveChangesAsync();
                    await LoadUsers();
                    ClearFields();
                    MessageBox.Show("User deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error deleting user: {ex.Message}";
                    MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateUserData()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("Username is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Email is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearFields()
        {
            Username = string.Empty;
            FullName = string.Empty;
            Email = string.Empty;
            Role = "User";
            PasswordHash = string.Empty;
            SelectedUser = null;
        }

        private string GeneratePasswordHash(string password)
        {
            // In a real application, use proper password hashing (e.g., BCrypt)
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        partial void OnSelectedUserChanged(User? value)
        {
            if (value != null)
            {
                Username = value.Username ?? string.Empty;
                FullName = value.FullName ?? string.Empty;
                Email = value.Email ?? string.Empty;
                Role = value.Role ?? "User";
                PasswordHash = string.Empty; // Don't populate password for security
            }
        }
    }
} 