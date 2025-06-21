using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models.EquipmentManagement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;

namespace InfraScheduler.Inventory.ViewModels
{
    public partial class EquipmentViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly System.Timers.Timer _searchTimer;
        private ObservableCollection<Equipment> _allEquipment;

        [ObservableProperty]
        private ObservableCollection<Equipment> equipment;

        [ObservableProperty]
        private string searchTerm = string.Empty;

        [ObservableProperty]
        private string equipmentName = string.Empty;

        [ObservableProperty]
        private string model = string.Empty;

        [ObservableProperty]
        private string serialNumber = string.Empty;

        [ObservableProperty]
        private string status = "Available";

        [ObservableProperty]
        private Equipment? selectedEquipment;

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

        public EquipmentViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _allEquipment = new ObservableCollection<Equipment>();
            Equipment = new ObservableCollection<Equipment>();

            // Initialize search timer
            _searchTimer = new System.Timers.Timer(300); // 300ms delay
            _searchTimer.Elapsed += (s, e) => 
            {
                _searchTimer.Stop();
                Application.Current.Dispatcher.Invoke(() => FilterEquipment());
            };

            LoadEquipment();
        }

        private async Task LoadEquipment()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _allEquipment.Clear();
                var equipment = await _context.Equipment.ToListAsync();
                foreach (var item in equipment)
                {
                    _allEquipment.Add(item);
                }
                FilterEquipment();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading equipment: {ex.Message}";
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

        private void FilterEquipment()
        {
            Equipment.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchTerm) 
                ? _allEquipment 
                : _allEquipment.Where(e => 
                    (e.Name?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.ModelNumber?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

            foreach (var item in filtered)
            {
                Equipment.Add(item);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateEquipmentData()) return;

            try
            {
                var equipment = new Equipment
                {
                    Name = EquipmentName,
                    ModelNumber = Model,
                    Status = Status
                };

                _context.Equipment.Add(equipment);
                await _context.SaveChangesAsync();
                await LoadEquipment();
                ClearFields();
                MessageBox.Show("Equipment added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving equipment: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Update()
        {
            if (SelectedEquipment == null || !ValidateEquipmentData()) return;

            try
            {
                SelectedEquipment.Name = EquipmentName;
                SelectedEquipment.ModelNumber = Model;
                SelectedEquipment.Status = Status;

                await _context.SaveChangesAsync();
                await LoadEquipment();
                ClearFields();
                MessageBox.Show("Equipment updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating equipment: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedEquipment == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete '{SelectedEquipment.Name}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Equipment.Remove(SelectedEquipment);
                    await _context.SaveChangesAsync();
                    await LoadEquipment();
                    ClearFields();
                    MessageBox.Show("Equipment deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error deleting equipment: {ex.Message}";
                    MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateEquipmentData()
        {
            if (string.IsNullOrWhiteSpace(EquipmentName))
            {
                MessageBox.Show("Equipment name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearFields()
        {
            EquipmentName = string.Empty;
            Model = string.Empty;
            SerialNumber = string.Empty;
            Status = "Available";
            SelectedEquipment = null;
        }

        partial void OnSelectedEquipmentChanged(Equipment? value)
        {
            if (value != null)
            {
                EquipmentName = value.Name ?? string.Empty;
                Model = value.ModelNumber ?? string.Empty;
                SerialNumber = string.Empty; // Equipment model doesn't have SerialNumber property
                Status = value.Status ?? "Available";
            }
        }
    }
} 