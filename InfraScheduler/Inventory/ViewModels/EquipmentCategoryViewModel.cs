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
    public partial class EquipmentCategoryViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly System.Timers.Timer _searchTimer;
        private ObservableCollection<EquipmentCategory> _allCategories;

        [ObservableProperty]
        private ObservableCollection<EquipmentCategory> equipmentCategories;

        [ObservableProperty]
        private string searchTerm = string.Empty;

        [ObservableProperty]
        private string categoryName = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private EquipmentCategory? selectedCategory;

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

        public EquipmentCategoryViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _allCategories = new ObservableCollection<EquipmentCategory>();
            EquipmentCategories = new ObservableCollection<EquipmentCategory>();

            // Initialize search timer
            _searchTimer = new System.Timers.Timer(300); // 300ms delay
            _searchTimer.Elapsed += (s, e) => 
            {
                _searchTimer.Stop();
                Application.Current.Dispatcher.Invoke(() => FilterCategories());
            };

            LoadCategories();
        }

        private async Task LoadCategories()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _allCategories.Clear();
                var categories = await _context.EquipmentCategories.ToListAsync();
                foreach (var category in categories)
                {
                    _allCategories.Add(category);
                }
                FilterCategories();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading equipment categories: {ex.Message}";
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

        private void FilterCategories()
        {
            EquipmentCategories.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchTerm) 
                ? _allCategories 
                : _allCategories.Where(c => 
                    (c.Name?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Description?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

            foreach (var category in filtered)
            {
                EquipmentCategories.Add(category);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateCategoryData()) return;

            try
            {
                var category = new EquipmentCategory
                {
                    Name = CategoryName,
                    Description = Description
                };

                _context.EquipmentCategories.Add(category);
                await _context.SaveChangesAsync();
                await LoadCategories();
                ClearFields();
                MessageBox.Show("Equipment category added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving equipment category: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Update()
        {
            if (SelectedCategory == null || !ValidateCategoryData()) return;

            try
            {
                SelectedCategory.Name = CategoryName;
                SelectedCategory.Description = Description;

                await _context.SaveChangesAsync();
                await LoadCategories();
                ClearFields();
                MessageBox.Show("Equipment category updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating equipment category: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedCategory == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete the category '{SelectedCategory.Name}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.EquipmentCategories.Remove(SelectedCategory);
                    await _context.SaveChangesAsync();
                    await LoadCategories();
                    ClearFields();
                    MessageBox.Show("Equipment category deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error deleting equipment category: {ex.Message}";
                    MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateCategoryData()
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                MessageBox.Show("Category name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearFields()
        {
            CategoryName = string.Empty;
            Description = string.Empty;
            SelectedCategory = null;
        }

        partial void OnSelectedCategoryChanged(EquipmentCategory? value)
        {
            if (value != null)
            {
                CategoryName = value.Name ?? string.Empty;
                Description = value.Description ?? string.Empty;
            }
        }
    }
} 