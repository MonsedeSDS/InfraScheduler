using InfraScheduler.Commands;
using InfraScheduler.Data;
using InfraScheduler.Models.EquipmentManagement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public class EquipmentCategoryViewModel : ViewModelBase
    {
        private readonly InfraSchedulerContext _context;
        private EquipmentCategory? _selectedEquipmentCategory;

        public EquipmentCategoryViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            EquipmentCategories = new ObservableCollection<EquipmentCategory>();
            
            InitializeCommands();
            _ = LoadData();
        }

        public ObservableCollection<EquipmentCategory> EquipmentCategories { get; set; }

        public EquipmentCategory? SelectedEquipmentCategory
        {
            get => _selectedEquipmentCategory;
            set
            {
                _selectedEquipmentCategory = value;
                OnPropertyChanged(nameof(SelectedEquipmentCategory));
                OnPropertyChanged(nameof(IsCategorySelected));
            }
        }

        public bool IsCategorySelected => SelectedEquipmentCategory != null;

        public RelayCommand LoadDataCommand { get; private set; } = null!;
        public RelayCommand AddCommand { get; private set; } = null!;
        public RelayCommand EditCommand { get; private set; } = null!;
        public RelayCommand DeleteCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            LoadDataCommand = new RelayCommand(async () => await LoadData());
            AddCommand = new RelayCommand(async () => await AddCategory());
            EditCommand = new RelayCommand(async () => await EditCategory());
            DeleteCommand = new RelayCommand(async () => await DeleteCategory());
        }

        private async Task LoadData()
        {
            try
            {
                EquipmentCategories.Clear();
                var categories = await _context.EquipmentCategories
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                
                foreach (var category in categories)
                {
                    EquipmentCategories.Add(category);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading equipment categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddCategory()
        {
            try
            {
                var category = new EquipmentCategory
                {
                    Name = "New Category",
                    Description = "Enter description here"
                };

                _context.EquipmentCategories.Add(category);
                _context.SaveChanges();

                EquipmentCategories.Add(category);
                SelectedEquipmentCategory = category;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding equipment category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EditCategory()
        {
            if (SelectedEquipmentCategory == null)
            {
                MessageBox.Show("Please select a category to edit.", "No Category Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // In a real application, you would open a dialog for editing
                // For now, we'll just save the current state
                _context.SaveChanges();
                MessageBox.Show("Category updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating equipment category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteCategory()
        {
            if (SelectedEquipmentCategory == null)
            {
                MessageBox.Show("Please select a category to delete.", "No Category Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete the category '{SelectedEquipmentCategory.Name}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Check if category is being used by any equipment
                    var equipmentCount = _context.Equipment
                        .Count(e => e.CategoryId == SelectedEquipmentCategory.Id);

                    if (equipmentCount > 0)
                    {
                        MessageBox.Show($"Cannot delete category '{SelectedEquipmentCategory.Name}' because it is used by {equipmentCount} equipment items.", 
                            "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _context.EquipmentCategories.Remove(SelectedEquipmentCategory);
                    _context.SaveChanges();

                    EquipmentCategories.Remove(SelectedEquipmentCategory);
                    SelectedEquipmentCategory = null;

                    MessageBox.Show("Category deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting equipment category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 