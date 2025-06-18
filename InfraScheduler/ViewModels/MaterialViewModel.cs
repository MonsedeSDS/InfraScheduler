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
using System.Windows.Input;
using InfraScheduler.Commands;

namespace InfraScheduler.ViewModels
{
    public partial class MaterialViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private string _name = string.Empty;
        private string _partNumber = string.Empty;
        private string _description = string.Empty;
        private decimal _unitPrice;
        private string _notes = string.Empty;
        private int _stockQuantity;
        private string _searchTerm = string.Empty;
        private Material? _selectedMaterial;
        private bool _isLoading;
        private ObservableCollection<Material> _materials = new();

        public ObservableCollection<Material> Materials
        {
            get => _materials;
            set
            {
                _materials = value;
                OnPropertyChanged(nameof(Materials));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PartNumber
        {
            get => _partNumber;
            set
            {
                if (_partNumber != value)
                {
                    _partNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged();
                }
            }
        }

        public int StockQuantity
        {
            get => _stockQuantity;
            set
            {
                if (_stockQuantity != value)
                {
                    _stockQuantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    OnPropertyChanged();
                    _ = LoadMaterialsAsync();
                }
            }
        }

        public Material? SelectedMaterial
        {
            get => _selectedMaterial;
            set
            {
                if (_selectedMaterial != value)
                {
                    _selectedMaterial = value;
                    OnPropertyChanged();
                    if (_selectedMaterial != null)
                    {
                        LoadMaterialDetails(_selectedMaterial);
                    }
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand AddMaterialCommand { get; }
        public ICommand UpdateMaterialCommand { get; }
        public ICommand DeleteMaterialCommand { get; }

        public MaterialViewModel(InfraSchedulerContext context)
        {
            _context = context;
            LoadDataCommand = new AsyncRelayCommand(LoadMaterialsAsync);
            AddMaterialCommand = new AsyncRelayCommand(AddMaterialAsync);
            UpdateMaterialCommand = new AsyncRelayCommand(UpdateMaterialAsync);
            DeleteMaterialCommand = new AsyncRelayCommand(DeleteMaterialAsync);
            _ = LoadMaterialsAsync();
        }

        private async Task LoadMaterialsAsync()
        {
            try
            {
                IsLoading = true;
                var query = _context.Materials.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    query = query.Where(m => 
                        m.Name.Contains(SearchTerm) || 
                        m.PartNumber.Contains(SearchTerm) || 
                        m.Description.Contains(SearchTerm));
                }

                var materials = await query.ToListAsync();
                Materials = new ObservableCollection<Material>(materials);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading materials: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadMaterialDetails(Material material)
        {
            Name = material.Name;
            PartNumber = material.PartNumber;
            Description = material.Description;
            UnitPrice = material.UnitPrice;
            Notes = material.Notes ?? string.Empty;
            StockQuantity = material.StockQuantity;
        }

        private async Task AddMaterialAsync()
        {
            try
            {
                IsLoading = true;
                var material = new Material
                {
                    Name = Name,
                    PartNumber = PartNumber,
                    Description = Description,
                    UnitPrice = UnitPrice,
                    Notes = Notes,
                    StockQuantity = StockQuantity
                };

                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
                await LoadMaterialsAsync();
                ClearFields();
                MessageBox.Show("Material added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding material: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateMaterialAsync()
        {
            if (SelectedMaterial == null)
            {
                MessageBox.Show("Please select a material to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                IsLoading = true;
                SelectedMaterial.Name = Name;
                SelectedMaterial.PartNumber = PartNumber;
                SelectedMaterial.Description = Description;
                SelectedMaterial.UnitPrice = UnitPrice;
                SelectedMaterial.Notes = Notes;
                SelectedMaterial.StockQuantity = StockQuantity;

                await _context.SaveChangesAsync();
                await LoadMaterialsAsync();
                MessageBox.Show("Material updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating material: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteMaterialAsync()
        {
            if (SelectedMaterial == null)
            {
                MessageBox.Show("Please select a material to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to delete this material?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    _context.Materials.Remove(SelectedMaterial);
                    await _context.SaveChangesAsync();
                    await LoadMaterialsAsync();
                    ClearFields();
                    MessageBox.Show("Material deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting material: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void ClearFields()
        {
            Name = string.Empty;
            PartNumber = string.Empty;
            Description = string.Empty;
            UnitPrice = 0;
            Notes = string.Empty;
            StockQuantity = 0;
            SelectedMaterial = null;
        }
    }
}
