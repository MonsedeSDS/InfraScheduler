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
    public partial class ResourceViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private int _categoryId;
        private string _searchTerm = string.Empty;
        private Resource? _selectedResource;
        private bool _isLoading;
        private ObservableCollection<Resource> _resources = new();
        private ObservableCollection<ResourceCategory> _categories = new();

        public ObservableCollection<Resource> Resources
        {
            get => _resources;
            set
            {
                _resources = value;
                OnPropertyChanged(nameof(Resources));
            }
        }

        public ObservableCollection<ResourceCategory> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
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

        public int CategoryId
        {
            get => _categoryId;
            set
            {
                if (_categoryId != value)
                {
                    _categoryId = value;
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
                    _ = LoadResourcesAsync();
                }
            }
        }

        public Resource? SelectedResource
        {
            get => _selectedResource;
            set
            {
                if (_selectedResource != value)
                {
                    _selectedResource = value;
                    OnPropertyChanged();
                    if (_selectedResource != null)
                    {
                        LoadResourceDetails(_selectedResource);
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
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public ResourceViewModel(InfraSchedulerContext context)
        {
            _context = context;
            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            SaveCommand = new RelayCommand(async () => await SaveAsync());
            DeleteCommand = new RelayCommand(async () => await DeleteAsync());
            ClearCommand = new RelayCommand(async () => await Task.Run(ClearFields));
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                await LoadResourcesAsync();
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadResourcesAsync()
        {
            try
            {
                var query = _context.Resources
                    .Include(r => r.Category)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    query = query.Where(r => 
                        r.Name.Contains(SearchTerm) || 
                        r.Description.Contains(SearchTerm));
                }

                var resources = await query.ToListAsync();
                Resources = new ObservableCollection<Resource>(resources);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading resources: {ex.Message}");
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _context.ResourceCategories.ToListAsync();
                Categories = new ObservableCollection<ResourceCategory>(categories);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}");
            }
        }

        private void LoadResourceDetails(Resource resource)
        {
            Name = resource.Name;
            Description = resource.Description;
            CategoryId = resource.CategoryId;
        }

        private async Task SaveAsync()
        {
            try
            {
                if (SelectedResource == null)
                {
                    SelectedResource = new Resource();
                    _context.Resources.Add(SelectedResource);
                }

                SelectedResource.Name = Name;
                SelectedResource.Description = Description;
                SelectedResource.CategoryId = CategoryId;

                await _context.SaveChangesAsync();
                await LoadResourcesAsync();
                ClearFields();
                MessageBox.Show("Resource saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving resource: {ex.Message}");
            }
        }

        private async Task DeleteAsync()
        {
            try
            {
                if (SelectedResource != null)
                {
                    _context.Resources.Remove(SelectedResource);
                    await _context.SaveChangesAsync();
                    await LoadResourcesAsync();
                    ClearFields();
                    MessageBox.Show("Resource deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting resource: {ex.Message}");
            }
        }

        private void ClearFields()
        {
            Name = string.Empty;
            Description = string.Empty;
            CategoryId = 0;
            SelectedResource = null;
        }
    }
}
