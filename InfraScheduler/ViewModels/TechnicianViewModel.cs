using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
using System.Windows.Input;
using Microsoft.Win32;

namespace InfraScheduler.ViewModels
{
    public partial class TechnicianViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly System.Timers.Timer _searchTimer;
        private ObservableCollection<Technician> _allTechnicians;
        private ObservableCollection<string> _availableRoles;
        private ObservableCollection<string> _selectedRoles;
        private string _selectedRole;

        [ObservableProperty]
        private ObservableCollection<Technician> technicians;

        [ObservableProperty]
        private ObservableCollection<string> availableCertifications;

        private ObservableCollection<string> _selectedCertifications = new();
        public ObservableCollection<string> SelectedCertifications
        {
            get => _selectedCertifications;
            set => SetProperty(ref _selectedCertifications, value);
        }

        private Dictionary<string, bool> _certificationsMarkedForRemoval = new();
        public Dictionary<string, bool> CertificationsMarkedForRemoval
        {
            get => _certificationsMarkedForRemoval;
            set => SetProperty(ref _certificationsMarkedForRemoval, value);
        }

        [ObservableProperty]
        private string searchTerm = string.Empty;

        [ObservableProperty]
        private string firstName = string.Empty;

        [ObservableProperty]
        private string lastName = string.Empty;

        [ObservableProperty]
        private string phone = string.Empty;

        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        private string selectedCertification = string.Empty;

        [ObservableProperty]
        private string newCertification = string.Empty;

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

        [ObservableProperty]
        private Technician? selectedTechnician;

        public ObservableCollection<string> AvailableRoles
        {
            get => _availableRoles;
            set => SetProperty(ref _availableRoles, value);
        }

        public ObservableCollection<string> SelectedRoles
        {
            get => _selectedRoles;
            set => SetProperty(ref _selectedRoles, value);
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public TechnicianViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _allTechnicians = new ObservableCollection<Technician>();
            Technicians = new ObservableCollection<Technician>();
            AvailableCertifications = new ObservableCollection<string>();
            SelectedCertifications = new ObservableCollection<string>();
            _availableRoles = new ObservableCollection<string>();
            _selectedRoles = new ObservableCollection<string>();

            // Initialize search timer
            _searchTimer = new System.Timers.Timer(300); // 300ms delay
            _searchTimer.Elapsed += (s, e) => 
            {
                _searchTimer.Stop();
                Application.Current.Dispatcher.Invoke(() => FilterTechnicians());
            };

            LoadTechnicians();
            LoadAvailableCertifications();
            LoadAvailableRoles();
        }

        private async Task LoadTechnicians()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _allTechnicians.Clear();
                var technicians = await _context.Technicians.ToListAsync();
                foreach (var technician in technicians)
                {
                    _allTechnicians.Add(technician);
                }
                FilterTechnicians();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading technicians: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAvailableCertifications()
        {
            try
            {
                var certs = await _context.Certifications
                    .OrderBy(c => c.Name)
                    .Select(c => c.Name)
                    .ToListAsync();

                AvailableCertifications.Clear();
                foreach (var cert in certs)
                {
                    AvailableCertifications.Add(cert);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading certifications: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAvailableRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .OrderBy(r => ((InfraScheduler.Models.Role)r).Name)
                    .Select(r => ((InfraScheduler.Models.Role)r).Name)
                    .ToListAsync();

                AvailableRoles.Clear();
                foreach (var role in roles)
                {
                    AvailableRoles.Add(role);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading roles: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSearchTermChanged(string value)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void FilterTechnicians()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                Technicians = new ObservableCollection<Technician>(_allTechnicians);
                return;
            }

            var searchTermLower = SearchTerm.ToLower();
            Technicians = new ObservableCollection<Technician>(
                _allTechnicians.Where(t =>
                    t.FirstName.ToLower().Contains(searchTermLower) ||
                    t.LastName.ToLower().Contains(searchTermLower)
                )
            );
        }

        [RelayCommand]
        private async Task AddTechnician()
        {
            try
            {
                if (!ValidateTechnicianData())
                    return;

                var newTechnician = new Technician
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Phone = Phone.Trim(),
                    Notes = Notes?.Trim(),
                    Certifications = string.Join(", ", SelectedCertifications)
                };

                _context.Technicians.Add(newTechnician);
                await _context.SaveChangesAsync();
                await LoadTechnicians();
                ClearFields();
                MessageBox.Show("Technician added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding technician: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task UpdateTechnician()
        {
            try
            {
                if (SelectedTechnician == null)
                {
                    MessageBox.Show("Please select a technician to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateTechnicianData())
                    return;

                SelectedTechnician.FirstName = FirstName.Trim();
                SelectedTechnician.LastName = LastName.Trim();
                SelectedTechnician.Phone = Phone.Trim();
                SelectedTechnician.Notes = Notes?.Trim();
                SelectedTechnician.Certifications = string.Join(", ", SelectedCertifications);
                SelectedTechnician.Roles = string.Join(", ", SelectedRoles);

                await _context.SaveChangesAsync();
                await LoadTechnicians();
                ClearFields();
                MessageBox.Show("Technician updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating technician: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteTechnician()
        {
            try
            {
                if (SelectedTechnician == null)
                {
                    MessageBox.Show("Please select a technician to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete {SelectedTechnician.FirstName} {SelectedTechnician.LastName}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Technicians.Remove(SelectedTechnician);
                    await _context.SaveChangesAsync();
                    await LoadTechnicians();
                    ClearFields();
                    MessageBox.Show("Technician deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting technician: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AddCertification()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SelectedCertification))
                {
                    MessageBox.Show("Please select a certification to add.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedCertifications.Contains(SelectedCertification))
                {
                    MessageBox.Show("This certification is already added.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedCertifications.Add(SelectedCertification);
                SelectedCertification = string.Empty;
                MessageBox.Show("Certification added. Don't forget to click 'Update' to save your changes.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding certification: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void RemoveCertification(string certification)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(certification))
                {
                    MessageBox.Show("Please select a certification to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to remove {certification}?",
                    "Confirm Remove",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Find and remove the exact certification
                    var certToRemove = SelectedCertifications.FirstOrDefault(c => c.Equals(certification, StringComparison.OrdinalIgnoreCase));
                    if (certToRemove != null)
                    {
                        SelectedCertifications.Remove(certToRemove);
                        if (SelectedTechnician != null)
                        {
                            SelectedTechnician.Certifications = string.Join(", ", SelectedCertifications);
                            _context.SaveChanges();
                            LoadTechnicians();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error removing certification: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddNewCertification()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewCertification))
                {
                    MessageBox.Show("Please enter a certification name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var trimmedCertification = NewCertification.Trim();
                if (trimmedCertification.Length > 100)
                {
                    MessageBox.Show("Certification name is too long (maximum 100 characters).", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (AvailableCertifications.Any(c => c.Equals(trimmedCertification, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("This certification already exists.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.Certifications.Add(new Certification { Name = trimmedCertification });
                await _context.SaveChangesAsync();

                AvailableCertifications.Add(trimmedCertification);
                SelectedCertification = trimmedCertification;
                NewCertification = string.Empty;

                MessageBox.Show("New certification added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding new certification: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void EditCertifications()
        {
            try
            {
                var window = new EditCertificationsWindow
                {
                    Owner = Application.Current.MainWindow
                };
                window.DataContext = new EditCertificationsViewModel(_context, window, SelectedCertifications);
                
                if (window.ShowDialog() == true)
                {
                    // Refresh certifications after editing
                    LoadAvailableCertifications();
                    OnPropertyChanged(nameof(SelectedCertifications));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error editing certifications: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateTechnicianData()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                MessageBox.Show("First name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                MessageBox.Show("Last name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Phone))
            {
                MessageBox.Show("Phone number is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Basic phone number validation
            if (!System.Text.RegularExpressions.Regex.IsMatch(Phone, @"^\d+$"))
            {
                MessageBox.Show("Please enter a valid phone number (numbers only).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearFields()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Phone = string.Empty;
            Notes = string.Empty;
            SelectedCertifications.Clear();
            _certificationsMarkedForRemoval.Clear();
            SelectedCertification = string.Empty;
            NewCertification = string.Empty;
            ErrorMessage = null;
            SelectedTechnician = null;
        }

        [RelayCommand]
        private void EditRoles()
        {
            try
            {
                var window = new EditRolesWindow
                {
                    Owner = Application.Current.MainWindow
                };
                window.DataContext = new EditRolesViewModel(_context, window, SelectedRoles);
                
                if (window.ShowDialog() == true)
                {
                    // Refresh roles after editing
                    LoadAvailableRoles();
                    OnPropertyChanged(nameof(SelectedRoles));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error editing roles: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSelectedRoles()
        {
            SelectedRoles.Clear();
            if (SelectedTechnician != null && !string.IsNullOrWhiteSpace(SelectedTechnician.Roles))
            {
                foreach (var role in SelectedTechnician.Roles.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                {
                    SelectedRoles.Add(role);
                }
            }
        }

        [RelayCommand]
        private void AddRole()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SelectedRole))
                {
                    MessageBox.Show("Please select a role to add.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedRoles.Contains(SelectedRole))
                {
                    MessageBox.Show("This role is already added.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedRoles.Add(SelectedRole);
                SelectedRole = string.Empty;
                MessageBox.Show("Role added. Don't forget to click 'Update' to save your changes.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding role: {ex.Message}";
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
                    if (SelectedTechnician != null)
                    {
                        SelectedTechnician.Roles = string.Join(", ", SelectedRoles);
                        _context.SaveChanges();
                        LoadTechnicians();
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
        private void Save()
        {
            try
            {
                _context.SaveChanges();
                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving changes: {ex.Message}";
            }
        }

        [RelayCommand]
        private void Delete()
        {
            if (SelectedTechnician == null) return;

            try
            {
                _context.Technicians.Remove(SelectedTechnician);
                _context.SaveChanges();
                Technicians.Remove(SelectedTechnician);
                SelectedTechnician = null;
                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting technician: {ex.Message}";
            }
        }

        partial void OnSelectedTechnicianChanged(Technician? value)
        {
            if (value != null)
            {
                FirstName = value.FirstName;
                LastName = value.LastName;
                Phone = value.Phone;
                Notes = value.Notes;
                UpdateSelectedRoles();
                UpdateSelectedCertifications();
            }
            else
            {
                ClearFields();
            }
        }

        private void UpdateSelectedCertifications()
        {
            SelectedCertifications.Clear();
            if (SelectedTechnician != null && !string.IsNullOrWhiteSpace(SelectedTechnician.Certifications))
            {
                foreach (var cert in SelectedTechnician.Certifications.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                {
                    SelectedCertifications.Add(cert);
                }
            }
        }
    }
}
