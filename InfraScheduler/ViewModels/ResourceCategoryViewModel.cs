using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;

namespace InfraScheduler.ViewModels
{
    public partial class ResourceCategoryViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private ResourceCategory? selectedCategory;

        public ObservableCollection<ResourceCategory> Categories { get; set; } = new();

        public ResourceCategoryViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");

            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _context = new InfraSchedulerContext(options);
            _context.Database.Migrate();

            LoadCategories();
        }

        private void LoadCategories()
        {
            Categories.Clear();
            foreach (var category in _context.ResourceCategories.ToList())
                Categories.Add(category);
        }

        [RelayCommand]
        private void AddCategory()
        {
            var newCategory = new ResourceCategory { Name = Name };
            _context.ResourceCategories.Add(newCategory);
            _context.SaveChanges();
            LoadCategories();
            ClearFields();
        }

        [RelayCommand]
        private void UpdateCategory()
        {
            if (SelectedCategory == null) return;

            SelectedCategory.Name = Name;
            _context.SaveChanges();
            LoadCategories();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteCategory()
        {
            if (SelectedCategory == null) return;

            _context.ResourceCategories.Remove(SelectedCategory);
            _context.SaveChanges();
            LoadCategories();
            ClearFields();
        }

        private void ClearFields()
        {
            Name = string.Empty;
            SelectedCategory = null;
        }
    }
}
