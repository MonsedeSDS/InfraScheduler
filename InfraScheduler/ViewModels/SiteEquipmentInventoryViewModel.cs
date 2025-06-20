using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using InfraScheduler.Models.EquipmentManagement;
using InfraScheduler.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class SiteEquipmentInventoryViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly SiteEquipmentQuery _siteEquipmentQuery;

        [ObservableProperty]
        private ObservableCollection<Site> availableSites = new();

        [ObservableProperty]
        private ObservableCollection<Site> sites = new();

        [ObservableProperty]
        private Site? selectedSite;

        [ObservableProperty]
        private ObservableCollection<SiteEquipmentSnapshot> siteEquipmentInventory = new();

        [ObservableProperty]
        private ObservableCollection<SiteEquipmentSnapshot> siteInventory = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private int totalEquipmentItems;

        public SiteEquipmentInventoryViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _siteEquipmentQuery = new SiteEquipmentQuery(_context);
            
            AvailableSites = new ObservableCollection<Site>();
            Sites = new ObservableCollection<Site>();
            SiteEquipmentInventory = new ObservableCollection<SiteEquipmentSnapshot>();
            SiteInventory = new ObservableCollection<SiteEquipmentSnapshot>();
            
            LoadAvailableSites();
            LoadSites();
        }

        private async void LoadAvailableSites()
        {
            try
            {
                IsLoading = true;
                var sites = await _context.Sites
                    .Include(s => s.SiteOwner)
                    .ToListAsync();

                AvailableSites.Clear();
                foreach (var site in sites)
                {
                    AvailableSites.Add(site);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sites: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadSites()
        {
            try
            {
                IsLoading = true;
                var sites = await _context.Sites
                    .Include(s => s.SiteOwner)
                    .ToListAsync();

                Sites.Clear();
                foreach (var site in sites)
                {
                    Sites.Add(site);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sites: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadInventory()
        {
            if (SelectedSite == null)
            {
                MessageBox.Show("Please select a site first.");
                return;
            }

            try
            {
                IsLoading = true;
                var inventory = await _siteEquipmentQuery.GetCurrentBySite(SelectedSite.Id);

                SiteEquipmentInventory.Clear();
                foreach (var item in inventory)
                {
                    SiteEquipmentInventory.Add(item);
                }

                TotalEquipmentItems = inventory.Sum(i => i.CurrentQty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inventory: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ExportToExcel()
        {
            if (SelectedSite == null)
            {
                MessageBox.Show("Please select a site first.");
                return;
            }

            if (!SiteEquipmentInventory.Any())
            {
                MessageBox.Show("Please load inventory first.");
                return;
            }

            try
            {
                // TODO: Implement Excel export functionality
                MessageBox.Show("Excel export functionality will be implemented later.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}");
            }
        }
    }
} 