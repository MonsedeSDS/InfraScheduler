using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System;

namespace InfraScheduler.Inventory.ViewModels
{
    public partial class ToolViewModel : ObservableObject
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
        [ObservableProperty] private Tool? selectedTool;
        [ObservableProperty] private ToolCategory? selectedCategory;
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

        [ObservableProperty] private ObservableCollection<Tool> tools = new();
        public ObservableCollection<ToolCategory> Categories { get; set; } = new();
        [ObservableProperty] private ObservableCollection<ToolAssignment> assignments = new();
        [ObservableProperty] private ObservableCollection<ToolMaintenance> maintenanceHistory = new();
        public ObservableCollection<Technician> Technicians { get; set; } = new();
        public ObservableCollection<Job> Jobs { get; set; } = new();

        public ToolViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");
            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _context = new InfraSchedulerContext(options);
            _context.Database.Migrate();

            LoadData();
        }

        private void LoadData()
        {
            var toolsList = _context.Tools
                .Include(t => t.Category)
                .Include(t => t.Assignments)
                .Include(t => t.MaintenanceHistory)
                .ToList();

            Tools.Clear();
            foreach (var tool in toolsList)
            {
                Tools.Add(tool);
            }

            Categories = new ObservableCollection<ToolCategory>(_context.ToolCategories.ToList());
            Technicians = new ObservableCollection<Technician>(_context.Technicians.ToList());
            Jobs = new ObservableCollection<Job>(_context.Jobs.ToList());
        }

        [RelayCommand]
        private void AddTool()
        {
            if (string.IsNullOrWhiteSpace(ItemType) || Quantity <= 0) 
            {
                MessageBox.Show("Please enter Item Type and Quantity greater than 0.");
                return;
            }

            try
            {
                // Check for existing model numbers to avoid duplicates
                var existingTools = _context.Tools.Where(t => t.Name == ItemType).ToList();
                var existingModelNumbers = existingTools.Select(t => t.ModelNumber).ToList();
                
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
                    var tool = new Tool
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
                        CategoryId = null // Remove category dependency
                    };

                    _context.Tools.Add(tool);
                }

                _context.SaveChanges();
                LoadData();
                ClearFields();
                
                MessageBox.Show($"Successfully added {Quantity} items of type '{ItemType}' with model numbers {ItemType}{nextNumber} to {ItemType}{nextNumber + Quantity - 1}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding tools: {ex.Message}");
            }
        }

        [RelayCommand]
        private void UpdateTool()
        {
            if (SelectedTool == null) return;

            SelectedTool.Name = ItemType;
            SelectedTool.Description = Description;
            SelectedTool.ModelNumber = ModelNumber;
            SelectedTool.Status = Status;
            SelectedTool.Condition = Condition;
            SelectedTool.LastServiceDate = LastServiceDate;
            SelectedTool.NextServiceDate = NextServiceDate;
            SelectedTool.CurrentLocation = CurrentLocation;
            SelectedTool.Notes = Notes;
            SelectedTool.CategoryId = CategoryId;

            _context.SaveChanges();
            LoadData();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteTool()
        {
            if (SelectedTool == null) return;

            _context.Tools.Remove(SelectedTool);
            _context.SaveChanges();
            LoadData();
            ClearFields();
        }

        [RelayCommand]
        private void CheckoutTool()
        {
            if (SelectedTool == null || !SelectedTechnicianId.HasValue) return;

            var assignment = new ToolAssignment
            {
                ToolId = SelectedTool.Id,
                TechnicianId = SelectedTechnicianId.Value,
                JobId = SelectedJobId,
                CheckoutDate = CheckoutDate,
                ExpectedReturnDate = ExpectedReturnDate,
                Notes = AssignmentNotes
            };

            _context.ToolAssignments.Add(assignment);
            
            // Update tool status
            SelectedTool.Status = "Checked Out";
            _context.SaveChanges();
            
            LoadData();
            ClearAssignmentFields();
        }

        [RelayCommand]
        private void ReturnTool()
        {
            if (SelectedTool == null) return;

            var assignment = _context.ToolAssignments
                .FirstOrDefault(a => a.ToolId == SelectedTool.Id && a.CheckoutDate != null);

            if (assignment != null)
            {
                // ReturnDate property doesn't exist in ToolAssignment model - removing
                SelectedTool.Status = "Available";
                _context.SaveChanges();
                LoadData();
                ClearAssignmentFields();
            }
        }

        [RelayCommand]
        private void AddMaintenance()
        {
            if (SelectedTool == null) return;

            var maintenance = new ToolMaintenance
            {
                ToolId = SelectedTool.Id,
                MaintenanceDate = MaintenanceDate,
                MaintenanceType = MaintenanceType,
                PerformedBy = PerformedBy,
                Description = Notes
            };

            _context.ToolMaintenances.Add(maintenance);
            
            // Update tool condition based on maintenance
            SelectedTool.Condition = UpdateToolCondition(SelectedTool.Condition, MaintenanceType);
            SelectedTool.LastServiceDate = MaintenanceDate;
            
            _context.SaveChanges();
            LoadData();
            ClearMaintenanceFields();
        }

        private string UpdateToolCondition(string currentCondition, string maintenanceType)
        {
            return maintenanceType.ToLower() switch
            {
                "repair" => "Good",
                "overhaul" => "New",
                _ => currentCondition
            };
        }

        [RelayCommand]
        private void SearchTools()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            var filteredTools = _context.Tools
                .Where(t => t.Name.Contains(SearchText) || 
                           t.ModelNumber.Contains(SearchText) ||
                           t.Description.Contains(SearchText))
                .ToList();

            Tools.Clear();
            foreach (var tool in filteredTools)
            {
                Tools.Add(tool);
            }
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
            SelectedTool = null;
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

        private void RefreshToolDetails()
        {
            if (SelectedTool != null)
            {
                ItemType = SelectedTool.Name;
                Description = SelectedTool.Description;
                ModelNumber = SelectedTool.ModelNumber;
                Status = SelectedTool.Status;
                Condition = SelectedTool.Condition;
                LastServiceDate = SelectedTool.LastServiceDate;
                NextServiceDate = SelectedTool.NextServiceDate;
                CurrentLocation = SelectedTool.CurrentLocation;
                Notes = SelectedTool.Notes;
                CategoryId = SelectedTool.CategoryId;

                // Load assignments and maintenance history
                Assignments = new ObservableCollection<ToolAssignment>(
                    _context.ToolAssignments.Where(a => a.ToolId == SelectedTool.Id).ToList());
                MaintenanceHistory = new ObservableCollection<ToolMaintenance>(
                    _context.ToolMaintenances.Where(m => m.ToolId == SelectedTool.Id).ToList());
            }
        }

        partial void OnSelectedToolChanged(Tool? value)
        {
            RefreshToolDetails();
        }

        [RelayCommand]
        private void AddItemType()
        {
            if (!string.IsNullOrWhiteSpace(NewItemType) && !ItemTypes.Contains(NewItemType))
            {
                ItemTypes.Add(NewItemType);
                NewItemType = string.Empty;
            }
        }

        [RelayCommand]
        private void DeleteItemType(string itemType)
        {
            ItemTypes.Remove(itemType);
        }
    }
} 