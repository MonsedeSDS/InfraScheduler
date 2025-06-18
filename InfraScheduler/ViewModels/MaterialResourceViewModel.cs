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

namespace InfraScheduler.ViewModels
{
    public partial class MaterialResourceViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private int _materialId;
        private double _quantity;
        private int _jobTaskId;
        private MaterialRequirement? _selectedMaterialRequirement;

        public ObservableCollection<MaterialRequirement> MaterialRequirements { get; set; } = new();
        public ObservableCollection<Material> Materials { get; set; } = new();
        public ObservableCollection<JobTask> JobTasks { get; set; } = new();

        public int MaterialId
        {
            get => _materialId;
            set
            {
                if (_materialId != value)
                {
                    _materialId = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public int JobTaskId
        {
            get => _jobTaskId;
            set
            {
                if (_jobTaskId != value)
                {
                    _jobTaskId = value;
                    OnPropertyChanged();
                }
            }
        }

        public MaterialRequirement? SelectedMaterialRequirement
        {
            get => _selectedMaterialRequirement;
            set
            {
                if (_selectedMaterialRequirement != value)
                {
                    _selectedMaterialRequirement = value;
                    OnPropertyChanged();
                }
            }
        }

        public MaterialResourceViewModel(InfraSchedulerContext context)
        {
            _context = context;
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var materialRequirements = await _context.MaterialRequirements
                .Include(mr => mr.Material)
                .Include(mr => mr.JobTask)
                .ToListAsync();

            MaterialRequirements.Clear();
            foreach (var requirement in materialRequirements)
            {
                MaterialRequirements.Add(requirement);
            }

            var materials = await _context.Materials.ToListAsync();
            Materials.Clear();
            foreach (var material in materials)
            {
                Materials.Add(material);
            }

            var jobTasks = await _context.JobTasks.ToListAsync();
            JobTasks.Clear();
            foreach (var task in jobTasks)
            {
                JobTasks.Add(task);
            }
        }

        [RelayCommand]
        private async Task AddMaterialRequirementAsync()
        {
            if (MaterialId <= 0)
            {
                MessageBox.Show("Please select a material.");
                return;
            }

            if (JobTaskId <= 0)
            {
                MessageBox.Show("Please select a job task.");
                return;
            }

            if (Quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.");
                return;
            }

            var material = await _context.Materials.FindAsync(MaterialId);
            if (material == null)
            {
                MessageBox.Show("Selected material not found.");
                return;
            }

            if (material.StockQuantity < (int)Quantity)
            {
                MessageBox.Show("Not enough stock available.");
                return;
            }

            var requirement = new MaterialRequirement
            {
                MaterialId = MaterialId,
                JobTaskId = JobTaskId,
                Quantity = Quantity,
                Unit = "pcs" // Default unit
            };

            _context.MaterialRequirements.Add(requirement);
            material.StockQuantity -= (int)Quantity;
            await _context.SaveChangesAsync();
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task DeleteMaterialRequirementAsync()
        {
            if (SelectedMaterialRequirement == null)
            {
                MessageBox.Show("Please select a material requirement to delete.");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this material requirement?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var material = await _context.Materials.FindAsync(SelectedMaterialRequirement.MaterialId);
                if (material != null)
                {
                    material.StockQuantity += (int)SelectedMaterialRequirement.Quantity;
                }

                _context.MaterialRequirements.Remove(SelectedMaterialRequirement);
                await _context.SaveChangesAsync();
                await LoadDataAsync();
            }
        }
    }
}
