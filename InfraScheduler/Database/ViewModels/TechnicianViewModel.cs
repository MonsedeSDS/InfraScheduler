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

namespace InfraScheduler.Database.ViewModels
{
    public partial class TechnicianViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly System.Timers.Timer _searchTimer;
        private ObservableCollection<Technician> _allTechnicians;

        [ObservableProperty]
        private ObservableCollection<Technician> technicians;

        [ObservableProperty]
        private string searchTerm = string.Empty;

        [ObservableProperty]
        private string firstName = string.Empty;

        [ObservableProperty]
        private string lastName = string.Empty;

        [ObservableProperty]
        private string phone = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private Technician? selectedTechnician;

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

        public TechnicianViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _allTechnicians = new ObservableCollection<Technician>();
            Technicians = new ObservableCollection<Technician>();

            // Initialize search timer
            _searchTimer = new System.Timers.Timer(300); // 300ms delay
            _searchTimer.Elapsed += (s, e) => 
            {
                _searchTimer.Stop();
                Application.Current.Dispatcher.Invoke(() => FilterTechnicians());
            };

            LoadTechnicians();
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

        partial void OnSearchTermChanged(string value)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void FilterTechnicians()
        {
            Technicians.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchTerm) 
                ? _allTechnicians 
                : _allTechnicians.Where(t => 
                    (t.FirstName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.LastName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

            foreach (var technician in filtered)
            {
                Technicians.Add(technician);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateTechnicianData()) return;

            try
            {
                var technician = new Technician
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Phone = Phone
                };

                _context.Technicians.Add(technician);
                await _context.SaveChangesAsync();
                await LoadTechnicians();
                ClearFields();
                MessageBox.Show("Technician added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving technician: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Update()
        {
            if (SelectedTechnician == null || !ValidateTechnicianData()) return;

            try
            {
                SelectedTechnician.FirstName = FirstName;
                SelectedTechnician.LastName = LastName;
                SelectedTechnician.Phone = Phone;

                await _context.SaveChangesAsync();
                await LoadTechnicians();
                ClearFields();
                MessageBox.Show("Technician updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating technician: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedTechnician == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete {SelectedTechnician.FirstName} {SelectedTechnician.LastName}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Technicians.Remove(SelectedTechnician);
                    await _context.SaveChangesAsync();
                    await LoadTechnicians();
                    ClearFields();
                    MessageBox.Show("Technician deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error deleting technician: {ex.Message}";
                    MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

            return true;
        }

        private void ClearFields()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            SelectedTechnician = null;
        }

        partial void OnSelectedTechnicianChanged(Technician? value)
        {
            if (value != null)
            {
                FirstName = value.FirstName ?? string.Empty;
                LastName = value.LastName ?? string.Empty;
                Phone = value.Phone ?? string.Empty;
            }
        }
    }
} 