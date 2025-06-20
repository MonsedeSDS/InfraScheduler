using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models.EquipmentManagement;
using InfraScheduler.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class LogisticsShippingViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly LogisticsService _logisticsService;

        [ObservableProperty]
        private ObservableCollection<EquipmentBatch> readyToShipBatches = new();

        [ObservableProperty]
        private EquipmentBatch? selectedBatch;

        [ObservableProperty]
        private ObservableCollection<EquipmentLine> equipmentLines = new();

        [ObservableProperty]
        private bool isLoading;

        public LogisticsShippingViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logisticsService = new LogisticsService(_context);
            
            ReadyToShipBatches = new ObservableCollection<EquipmentBatch>();
            EquipmentLines = new ObservableCollection<EquipmentLine>();
            
            LoadReadyToShipBatches();
        }

        private async void LoadReadyToShipBatches()
        {
            try
            {
                IsLoading = true;
                var batches = await _context.EquipmentBatches
                    .Include(b => b.Job)
                    .ThenInclude(j => j.Site)
                    .Include(b => b.Lines)
                    .Where(b => b.Status == "Created" && b.Lines.All(l => l.Status == EquipmentStatus.SDSWarehouse))
                    .ToListAsync();

                ReadyToShipBatches.Clear();
                foreach (var batch in batches)
                {
                    ReadyToShipBatches.Add(batch);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading batches: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSelectedBatchChanged(EquipmentBatch? value)
        {
            if (value != null)
            {
                LoadEquipmentLines(value.Id);
            }
            else
            {
                EquipmentLines.Clear();
            }
        }

        private async void LoadEquipmentLines(int batchId)
        {
            try
            {
                var lines = await _context.EquipmentLines
                    .Include(l => l.EquipmentType)
                    .Where(l => l.BatchId == batchId)
                    .ToListAsync();

                EquipmentLines.Clear();
                foreach (var line in lines)
                {
                    EquipmentLines.Add(line);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading equipment lines: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MarkShipped()
        {
            if (SelectedBatch == null)
            {
                MessageBox.Show("Please select a batch first.");
                return;
            }

            try
            {
                IsLoading = true;
                await _logisticsService.MarkShipped(SelectedBatch.Id);
                
                MessageBox.Show("Batch marked as shipped successfully!");
                
                // Refresh the batches list
                LoadReadyToShipBatches();
                SelectedBatch = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error marking batch as shipped: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 