using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace InfraScheduler.Database.ViewModels
{
    public partial class SiteViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private string _siteName = string.Empty;
        [ObservableProperty] private string _siteCode = string.Empty;
        [ObservableProperty] private string _address = string.Empty;
        [ObservableProperty] private double? _latitude;
        [ObservableProperty] private double? _longitude;
        [ObservableProperty] private string _searchTerm = string.Empty;
        [ObservableProperty] private Site? _selectedSite;

        [ObservableProperty] private ObservableCollection<Site> _sites = new();

        public SiteViewModel(InfraSchedulerContext context)
        {
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            var sites = _context.Sites.ToList();

            Sites.Clear();
            foreach (var site in sites)
            {
                Sites.Add(site);
            }
        }

        [RelayCommand]
        private async Task AddSite()
        {
            try
            {
                var site = new Site
                {
                    SiteName = SiteName,
                    SiteCode = SiteCode,
                    Address = Address,
                    Latitude = Latitude ?? 0.0,
                    Longitude = Longitude ?? 0.0
                };

                _context.Sites.Add(site);
                await _context.SaveChangesAsync();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding site: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task UpdateSite()
        {
            if (SelectedSite == null) return;

            try
            {
                SelectedSite.SiteName = SiteName;
                SelectedSite.SiteCode = SiteCode;
                SelectedSite.Address = Address;
                SelectedSite.Latitude = Latitude ?? 0.0;
                SelectedSite.Longitude = Longitude ?? 0.0;

                await _context.SaveChangesAsync();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating site: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DeleteSite()
        {
            if (SelectedSite == null) return;

            try
            {
                _context.Sites.Remove(SelectedSite);
                await _context.SaveChangesAsync();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting site: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            SiteName = string.Empty;
            SiteCode = string.Empty;
            Address = string.Empty;
            Latitude = null;
            Longitude = null;
            SelectedSite = null;
        }

        partial void OnSelectedSiteChanged(Site? value)
        {
            if (value != null)
            {
                SiteName = value.SiteName;
                SiteCode = value.SiteCode;
                Address = value.Address;
                Latitude = value.Latitude;
                Longitude = value.Longitude;
            }
        }

        partial void OnSearchTermChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                LoadData();
                return;
            }

            var filteredSites = _context.Sites
                .Where(s => s.SiteName.Contains(value) || 
                           s.SiteCode.Contains(value) ||
                           s.Address.Contains(value))
                .ToList();

            Sites.Clear();
            foreach (var site in filteredSites)
            {
                Sites.Add(site);
            }
        }
    }
} 