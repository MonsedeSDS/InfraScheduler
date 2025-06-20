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
    public partial class ReceivingViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly ReceivingService _receivingService;

        [ObservableProperty]
        private ObservableCollection<EquipmentBatch> availableBatches = new();

        [ObservableProperty]
        private ObservableCollection<EquipmentBatch> pendingBatches = new();

        [ObservableProperty]
        private EquipmentBatch? selectedBatch;

        [ObservableProperty]
        private ObservableCollection<EquipmentLine> equipmentLines = new();

        [ObservableProperty]
        private EquipmentLine? selectedEquipmentLine;

        [ObservableProperty]
        private bool isLoading;

        public ReceivingViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _receivingService = new ReceivingService(_context);
            
            AvailableBatches = new ObservableCollection<EquipmentBatch>();
            PendingBatches = new ObservableCollection<EquipmentBatch>();
            EquipmentLines = new ObservableCollection<EquipmentLine>();
            
            LoadAvailableBatches();
            LoadPendingBatches();
        }

        private async void LoadAvailableBatches()
        {
            try
            {
                IsLoading = true;
                var batches = await _context.EquipmentBatches
                    .Include(b => b.Job)
                    .ThenInclude(j => j.Site)
                    .Include(b => b.Lines)
                    .Where(b => b.Status == "Created" || b.Status == "In Transit")
                    .ToListAsync();

                AvailableBatches.Clear();
                foreach (var batch in batches)
                {
                    AvailableBatches.Add(batch);
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

        private async Task LoadPendingBatches()
        {
            try
            {
                IsLoading = true;
                var batches = await _context.EquipmentBatches
                    .Include(b => b.Job)
                    .ThenInclude(j => j.Site)
                    .Include(b => b.Lines)
                    .Where(b => b.Status == "Pending")
                    .ToListAsync();

                PendingBatches.Clear();
                foreach (var batch in batches)
                {
                    PendingBatches.Add(batch);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pending batches: {ex.Message}");
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
        private async Task ReceiveAllLines()
        {
            if (SelectedBatch == null)
            {
                MessageBox.Show("Please select a batch first.");
                return;
            }

            try
            {
                IsLoading = true;
                foreach (var line in EquipmentLines)
                {
                    if (line.Status == EquipmentStatus.ClientWarehouse)
                    {
                        await _receivingService.ReceiveLine(line.Id, line.ReceivedQty);
                    }
                }

                MessageBox.Show("All lines received successfully!");
                LoadEquipmentLines(SelectedBatch.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error receiving lines: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ReceiveSelectedLine()
        {
            if (SelectedEquipmentLine == null)
            {
                MessageBox.Show("Please select an equipment line first.");
                return;
            }

            try
            {
                IsLoading = true;
                await _receivingService.ReceiveLine(SelectedEquipmentLine.Id, SelectedEquipmentLine.ReceivedQty);
                
                MessageBox.Show("Line received successfully!");
                LoadEquipmentLines(SelectedBatch!.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error receiving line: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ViewDiscrepancies()
        {
            if (SelectedBatch == null)
            {
                MessageBox.Show("Please select a batch first.");
                return;
            }

            try
            {
                var discrepancies = await _context.EquipmentDiscrepancies
                    .Include(d => d.Line)
                    .ThenInclude(l => l.EquipmentType)
                    .Where(d => d.Line.BatchId == SelectedBatch.Id)
                    .ToListAsync();

                if (discrepancies.Any())
                {
                    var message = "Discrepancies found:\n\n";
                    foreach (var discrepancy in discrepancies)
                    {
                        message += $"Equipment: {discrepancy.Line.EquipmentType.Name}\n";
                        message += $"Planned: {discrepancy.PlannedQty}, Received: {discrepancy.ReceivedQty}\n";
                        message += $"Note: {discrepancy.Note}\n\n";
                    }
                    MessageBox.Show(message, "Discrepancies");
                }
                else
                {
                    MessageBox.Show("No discrepancies found for this batch.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading discrepancies: {ex.Message}");
            }
        }
    }
} 