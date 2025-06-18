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
    public partial class SiteViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private string _siteName = string.Empty;
        private string _address = string.Empty;
        private string _siteCode = string.Empty;
        private double _latitude;
        private double _longitude;
        private int _siteOwnerId;
        private string _searchTerm = string.Empty;
        private Site? _selectedSite;
        private bool _isLoading;
        private ObservableCollection<Site> _sites = new();
        private ObservableCollection<SiteOwner> _siteOwners = new();
        private ObservableCollection<Client> _selectedTenants = new();
        private ObservableCollection<Client> _availableClients = new();
        private Client? _selectedClient;

        public ObservableCollection<Site> Sites
        {
            get => _sites;
            set
            {
                _sites = value;
                OnPropertyChanged(nameof(Sites));
            }
        }

        public ObservableCollection<SiteOwner> SiteOwners
        {
            get => _siteOwners;
            set
            {
                _siteOwners = value;
                OnPropertyChanged(nameof(SiteOwners));
            }
        }

        public ObservableCollection<Client> SelectedTenants
        {
            get => _selectedTenants;
            set
            {
                _selectedTenants = value;
                OnPropertyChanged(nameof(SelectedTenants));
            }
        }

        public ObservableCollection<Client> AvailableClients
        {
            get => _availableClients;
            set
            {
                _availableClients = value;
                OnPropertyChanged(nameof(AvailableClients));
            }
        }

        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
            }
        }

        public string SiteName
        {
            get => _siteName;
            set
            {
                if (_siteName != value)
                {
                    _siteName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                if (_address != value)
                {
                    _address = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SiteCode
        {
            get => _siteCode;
            set
            {
                if (_siteCode != value)
                {
                    _siteCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Latitude
        {
            get => _latitude;
            set
            {
                if (_latitude != value)
                {
                    _latitude = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Longitude
        {
            get => _longitude;
            set
            {
                if (_longitude != value)
                {
                    _longitude = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SiteOwnerId
        {
            get => _siteOwnerId;
            set
            {
                if (_siteOwnerId != value)
                {
                    _siteOwnerId = value;
                    OnPropertyChanged(nameof(SiteOwnerId));
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
                    _ = LoadSitesAsync();
                }
            }
        }

        public Site? SelectedSite
        {
            get => _selectedSite;
            set
            {
                if (_selectedSite != value)
                {
                    _selectedSite = value;
                    OnPropertyChanged();
                    if (_selectedSite != null)
                    {
                        LoadSiteDetails(_selectedSite);
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
        public ICommand AddSiteCommand { get; }
        public ICommand UpdateSiteCommand { get; }
        public ICommand DeleteSiteCommand { get; }

        public SiteViewModel(InfraSchedulerContext context)
        {
            _context = context;
            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            AddSiteCommand = new RelayCommand(async () => await AddSiteAsync());
            UpdateSiteCommand = new RelayCommand(async () => await UpdateSiteAsync());
            DeleteSiteCommand = new RelayCommand(async () => await DeleteSiteAsync());
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                await LoadSitesAsync();
                await LoadSiteOwnersFromClientsAsync();
                await LoadAvailableClientsAsync();
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

        private async Task LoadSitesAsync()
        {
            var query = _context.Sites
                .Include(s => s.SiteOwner)
                .Include(s => s.SiteTenants)
                    .ThenInclude(st => st.Client)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(s =>
                    s.SiteName.Contains(SearchTerm) ||
                    s.SiteCode.Contains(SearchTerm) ||
                    s.Address.Contains(SearchTerm) ||
                    s.SiteOwner.CompanyName.Contains(SearchTerm));
            }

            var sites = await query.ToListAsync();
            Sites = new ObservableCollection<Site>(sites);
        }

        private async Task LoadSiteOwnersFromClientsAsync()
        {
            var clients = await _context.Clients.ToListAsync();
            var siteOwners = clients.Select(c => new SiteOwner
            {
                Id = c.Id,
                CompanyName = c.Name,
                ContactPerson = c.ContactPerson,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address
            }).ToList();
            SiteOwners = new ObservableCollection<SiteOwner>(siteOwners);
        }

        private async Task LoadAvailableClientsAsync()
        {
            var clients = await _context.Clients.ToListAsync();
            AvailableClients = new ObservableCollection<Client>(clients);
        }

        private void LoadSiteDetails(Site site)
        {
            SiteName = site.SiteName;
            Address = site.Address;
            SiteCode = site.SiteCode;
            Latitude = site.Latitude;
            Longitude = site.Longitude;
            SiteOwnerId = site.ClientId;
            OnPropertyChanged(nameof(SiteOwnerId));

            // Load tenants
            SelectedTenants.Clear();
            foreach (var tenant in site.SiteTenants)
            {
                if (tenant.Client != null)
                {
                    SelectedTenants.Add(tenant.Client);
                }
            }
        }

        private async Task AddSiteAsync()
        {
            if (string.IsNullOrWhiteSpace(SiteName))
            {
                MessageBox.Show("Please enter a site name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                MessageBox.Show("Please enter an address.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SiteCode))
            {
                MessageBox.Show("Please enter a site code.");
                return;
            }

            if (SiteOwnerId <= 0)
            {
                MessageBox.Show("Please select a site owner.");
                return;
            }

            var client = await _context.Clients.FindAsync(SiteOwnerId);
            if (client == null)
            {
                MessageBox.Show("Selected site owner not found.");
                return;
            }

            try
            {
                // First, ensure the SiteOwner exists
                var siteOwner = await _context.SiteOwners.FindAsync(SiteOwnerId);
                if (siteOwner == null)
                {
                    // Create a new SiteOwner from the Client data
                    siteOwner = new SiteOwner
                    {
                        Id = client.Id,
                        CompanyName = client.Name,
                        ContactPerson = client.ContactPerson,
                        Phone = client.Phone,
                        Email = client.Email,
                        Address = client.Address
                    };
                    _context.SiteOwners.Add(siteOwner);
                    await _context.SaveChangesAsync();
                }

                var site = new Site
                {
                    SiteName = SiteName,
                    Address = Address,
                    SiteCode = SiteCode,
                    Latitude = Latitude,
                    Longitude = Longitude,
                    ClientId = SiteOwnerId,
                    SiteOwnerId = SiteOwnerId
                };

                _context.Sites.Add(site);
                await _context.SaveChangesAsync();

                // Add selected tenants
                foreach (var tenant in SelectedTenants)
                {
                    var siteTenant = new SiteTenant
                    {
                        SiteId = site.Id,
                        ClientId = tenant.Id
                    };
                    _context.SiteTenants.Add(siteTenant);
                }
                await _context.SaveChangesAsync();

                await LoadSitesAsync();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding site: {ex.Message}");
                _context.ChangeTracker.Clear();
            }
        }

        private async Task UpdateSiteAsync()
        {
            if (SelectedSite == null)
            {
                MessageBox.Show("Please select a site to update.");
                return;
            }

            try
            {
                IsLoading = true;
                SelectedSite.SiteName = SiteName;
                SelectedSite.Address = Address;
                SelectedSite.SiteCode = SiteCode;
                SelectedSite.Latitude = Latitude;
                SelectedSite.Longitude = Longitude;
                SelectedSite.SiteOwnerId = SiteOwnerId;

                await _context.SaveChangesAsync();
                await LoadSitesAsync();
                MessageBox.Show("Site updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating site: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteSiteAsync()
        {
            if (SelectedSite == null)
            {
                MessageBox.Show("Please select a site to delete.");
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to delete this site?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    _context.Sites.Remove(SelectedSite);
                    await _context.SaveChangesAsync();
                    await LoadSitesAsync();
                    ClearFields();
                    MessageBox.Show("Site deleted successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting site: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        [RelayCommand]
        private void AddTenant()
        {
            if (SelectedClient == null) return;
            if (!SelectedTenants.Contains(SelectedClient))
            {
                SelectedTenants.Add(SelectedClient);
            }
            SelectedClient = null;
        }

        [RelayCommand]
        private void RemoveTenant(Client client)
        {
            if (client != null)
            {
                SelectedTenants.Remove(client);
            }
        }

        private void ClearFields()
        {
            SiteName = string.Empty;
            Address = string.Empty;
            SiteCode = string.Empty;
            Latitude = 0;
            Longitude = 0;
            SiteOwnerId = 0;
            OnPropertyChanged(nameof(SiteOwnerId));
            SelectedSite = null;
            SelectedTenants.Clear();
        }
    }
}
