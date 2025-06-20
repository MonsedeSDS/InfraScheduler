using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Models.EquipmentManagement;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System;

namespace InfraScheduler.ViewModels
{
    public partial class EquipmentViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private string itemType = string.Empty;
        [ObservableProperty] private string description = string.Empty;
        [ObservableProperty] private string modelNumber = string.Empty;
        [ObservableProperty] private int quantity = 1;
        [ObservableProperty] private string status = "Available";
        [ObservableProperty] private string condition = "New";
        [ObservableProperty] private DateTime? lastServiceDate;
        [ObservableProperty] private DateTime? nextServiceDate;
        [ObservableProperty] private string currentLocation = string.Empty;
        [ObservableProperty] private string notes = string.Empty;
        [ObservableProperty] private int? categoryId;
        [ObservableProperty] private Equipment? selectedEquipment;
        [ObservableProperty] private EquipmentCategory? selectedCategory;
        [ObservableProperty] private string searchText = string.Empty;

        // Assignment properties
        [ObservableProperty] private int? selectedTechnicianId;
        [ObservableProperty] private int? selectedJobId;
        [ObservableProperty] private DateTime checkoutDate = DateTime.Now;
        [ObservableProperty] private DateTime? expectedReturnDate;
        [ObservableProperty] private string assignmentNotes = string.Empty;

        // Maintenance properties
        [ObservableProperty] private DateTime maintenanceDate = DateTime.Now;
        [ObservableProperty] private string maintenanceType = "Regular";
        [ObservableProperty] private string performedBy = string.Empty;

        [ObservableProperty] private string newItemType = string.Empty;
        public ObservableCollection<string> ItemTypes { get; set; } = new();

        [ObservableProperty] private ObservableCollection<Equipment> equipment = new();
        public ObservableCollection<EquipmentCategory> Categories { get; set; } = new();
        public ObservableCollection<Technician> Technicians { get; set; } = new();
        public ObservableCollection<Job> Jobs { get; set; } = new();

        public EquipmentViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            LoadData();
        }

        private void LoadData()
        {
            var equipmentList = _context.Equipment
                .Include(e => e.Category)
                .ToList();

            Equipment.Clear();
            foreach (var item in equipmentList)
            {
                Equipment.Add(item);
            }

            Categories = new ObservableCollection<EquipmentCategory>(_context.EquipmentCategories.ToList());
            Technicians = new ObservableCollection<Technician>(_context.Technicians.ToList());
            Jobs = new ObservableCollection<Job>(_context.Jobs.ToList());
        }

        [RelayCommand]
        private void AddEquipment()
        {
            if (string.IsNullOrWhiteSpace(ItemType) || Quantity <= 0) 
            {
                MessageBox.Show("Please enter Item Type and Quantity greater than 0.");
                return;
            }

            try
            {
                // Check for existing model numbers to avoid duplicates
                var existingEquipment = _context.Equipment.Where(e => e.Name == ItemType).ToList();
                var existingModelNumbers = existingEquipment.Select(e => e.ModelNumber).ToList();
                
                // Find the highest number used for this item type
                int nextNumber = 1;
                if (existingModelNumbers.Any())
                {
                    var numbers = existingModelNumbers
                        .Where(mn => mn.StartsWith(ItemType))
                        .Select(mn => 
                        {
                            var suffix = mn.Substring(ItemType.Length);
                            return int.TryParse(suffix, out var num) ? num : 0;
                        })
                        .Where(num => num > 0)
                        .ToList();
                    
                    nextNumber = numbers.Any() ? numbers.Max() + 1 : 1;
                }

                // Verify we have enough space for the batch
                var requiredNumbers = Enumerable.Range(nextNumber, Quantity).ToList();
                var conflictingNumbers = requiredNumbers
                    .Where(num => existingModelNumbers.Contains($"{ItemType}{num}"))
                    .ToList();

                if (conflictingNumbers.Any())
                {
                    MessageBox.Show($"Cannot create batch: Model numbers {string.Join(", ", conflictingNumbers.Select(n => $"{ItemType}{n}"))} already exist.");
                    return;
                }

                for (int i = 0; i < Quantity; i++)
                {
                    var equipment = new Equipment
                    {
                        Name = ItemType,
                        Description = Description,
                        ModelNumber = $"{ItemType}{nextNumber + i}",
                        Status = "Available", // Always set to Available for new items
                        Condition = Condition,
                        LastServiceDate = LastServiceDate,
                        NextServiceDate = NextServiceDate,
                        CurrentLocation = CurrentLocation,
                        Notes = Notes,
                        CategoryId = CategoryId
                    };

                    _context.Equipment.Add(equipment);
                }

                _context.SaveChanges();
                LoadData();
                ClearFields();
                
                MessageBox.Show($"Successfully added {Quantity} items of type '{ItemType}' with model numbers {ItemType}{nextNumber} to {ItemType}{nextNumber + Quantity - 1}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding equipment: {ex.Message}");
            }
        }

        [RelayCommand]
        private void UpdateEquipment()
        {
            if (SelectedEquipment == null) return;

            SelectedEquipment.Name = ItemType;
            SelectedEquipment.Description = Description;
            SelectedEquipment.ModelNumber = ModelNumber;
            SelectedEquipment.Status = Status;
            SelectedEquipment.Condition = Condition;
            SelectedEquipment.LastServiceDate = LastServiceDate;
            SelectedEquipment.NextServiceDate = NextServiceDate;
            SelectedEquipment.CurrentLocation = CurrentLocation;
            SelectedEquipment.Notes = Notes;
            SelectedEquipment.CategoryId = CategoryId;

            _context.SaveChanges();
            LoadData();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteEquipment()
        {
            if (SelectedEquipment == null) return;

            _context.Equipment.Remove(SelectedEquipment);
            _context.SaveChanges();
            LoadData();
            ClearFields();
        }

        [RelayCommand]
        private void CheckoutEquipment()
        {
            if (SelectedEquipment == null || !SelectedTechnicianId.HasValue) return;

            SelectedEquipment.Status = "In Use";
            SelectedEquipment.AssignedToTechnicianId = SelectedTechnicianId;
            SelectedEquipment.AssignedToJobId = SelectedJobId;

            _context.SaveChanges();
            LoadData();
            ClearAssignmentFields();
        }

        [RelayCommand]
        private void ReturnEquipment()
        {
            if (SelectedEquipment == null) return;

            SelectedEquipment.Status = "Available";
            SelectedEquipment.AssignedToTechnicianId = null;
            SelectedEquipment.AssignedToJobId = null;

            _context.SaveChanges();
            LoadData();
            ClearAssignmentFields();
        }

        [RelayCommand]
        private void SearchEquipment()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            Equipment = new ObservableCollection<Equipment>(_context.Equipment
                .Include(e => e.Category)
                .Where(e => e.Name.Contains(SearchText) ||
                           e.ModelNumber.Contains(SearchText))
                .ToList());
        }

        private void ClearFields()
        {
            ItemType = string.Empty;
            Description = string.Empty;
            ModelNumber = string.Empty;
            Quantity = 1;
            Status = "Available";
            Condition = "New";
            LastServiceDate = null;
            NextServiceDate = null;
            CurrentLocation = string.Empty;
            Notes = string.Empty;
            CategoryId = null;
            SelectedEquipment = null;
            SelectedCategory = null;
            SearchText = string.Empty;
        }

        private void ClearAssignmentFields()
        {
            SelectedTechnicianId = null;
            SelectedJobId = null;
            CheckoutDate = DateTime.Now;
            ExpectedReturnDate = null;
            AssignmentNotes = string.Empty;
        }

        private void ClearMaintenanceFields()
        {
            MaintenanceDate = DateTime.Now;
            MaintenanceType = "Regular";
            PerformedBy = string.Empty;
        }

        partial void OnSelectedEquipmentChanged(Equipment? value)
        {
            if (value == null) return;

            ItemType = value.Name;
            Description = value.Description;
            ModelNumber = value.ModelNumber;
            Status = value.Status;
            Condition = value.Condition;
            LastServiceDate = value.LastServiceDate;
            NextServiceDate = value.NextServiceDate;
            CurrentLocation = value.CurrentLocation;
            Notes = value.Notes;
            CategoryId = value.CategoryId;
            SelectedCategory = value.Category;
        }

        [RelayCommand]
        private void AddItemType()
        {
            if (string.IsNullOrWhiteSpace(NewItemType)) return;
            ItemTypes.Add(NewItemType);
            NewItemType = string.Empty;
        }

        [RelayCommand]
        private void DeleteItemType(string itemType)
        {
            ItemTypes.Remove(itemType);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 