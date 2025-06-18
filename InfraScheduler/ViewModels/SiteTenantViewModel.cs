using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class SiteTenantViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private int siteId;
        [ObservableProperty] private int clientId;
        [ObservableProperty] private SiteTenant? selectedSiteTenant;

        public ObservableCollection<SiteTenant> SiteTenants { get; set; } = new();
        public ObservableCollection<Site> Sites { get; set; } = new();
        public ObservableCollection<Client> Clients { get; set; } = new();

        public SiteTenantViewModel()
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
            SiteTenants.Clear();
            Sites.Clear();
            Clients.Clear();

            foreach (var siteTenant in _context.SiteTenants
                                               .Include(st => st.Site)
                                               .Include(st => st.Client)
                                               .ToList())
            {
                SiteTenants.Add(siteTenant);
            }

            foreach (var site in _context.Sites.ToList())
            {
                Sites.Add(site);
            }

            foreach (var client in _context.Clients.ToList())
            {
                Clients.Add(client);
            }
        }

        [RelayCommand]
        private void AddSiteTenant()
        {
            if (SiteId == 0 || ClientId == 0)
            {
                MessageBox.Show("Please select both a Site and a Client.");
                return;
            }

            var newSiteTenant = new SiteTenant
            {
                SiteId = SiteId,
                ClientId = ClientId
            };

            _context.SiteTenants.Add(newSiteTenant);
            _context.SaveChanges();
            LoadData();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteSiteTenant()
        {
            if (SelectedSiteTenant == null)
            {
                MessageBox.Show("Please select a SiteTenant to delete.");
                return;
            }

            _context.SiteTenants.Remove(SelectedSiteTenant);
            _context.SaveChanges();
            LoadData();
            ClearFields();
        }

        private void ClearFields()
        {
            SiteId = 0;
            ClientId = 0;
            SelectedSiteTenant = null;
        }
    }
}
