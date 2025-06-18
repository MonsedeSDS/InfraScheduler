using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class SiteOwnerViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private string companyName = string.Empty;
        [ObservableProperty] private string contactPerson = string.Empty;
        [ObservableProperty] private string phone = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string address = string.Empty;

        [ObservableProperty] private SiteOwner? selectedSiteOwner;

        public ObservableCollection<SiteOwner> SiteOwners { get; set; } = new();

        public SiteOwnerViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");

            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _context = new InfraSchedulerContext(options);
            _context.Database.Migrate();
            LoadSiteOwners();
        }

        private void LoadSiteOwners()
        {
            SiteOwners.Clear();
            foreach (var owner in _context.SiteOwners.ToList())
            {
                SiteOwners.Add(owner);
            }
        }

        [RelayCommand]
        private void AddSiteOwner()
        {
            var newOwner = new SiteOwner
            {
                CompanyName = CompanyName,
                ContactPerson = ContactPerson,
                Phone = Phone,
                Email = Email,
                Address = Address
            };

            _context.SiteOwners.Add(newOwner);
            _context.SaveChanges();
            LoadSiteOwners();
            ClearFields();
        }

        [RelayCommand]
        private void UpdateSiteOwner()
        {
            if (SelectedSiteOwner == null)
            {
                MessageBox.Show("Please select a site owner to update.");
                return;
            }

            SelectedSiteOwner.CompanyName = CompanyName;
            SelectedSiteOwner.ContactPerson = ContactPerson;
            SelectedSiteOwner.Phone = Phone;
            SelectedSiteOwner.Email = Email;
            SelectedSiteOwner.Address = Address;

            _context.SaveChanges();
            LoadSiteOwners();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteSiteOwner()
        {
            if (SelectedSiteOwner == null)
            {
                MessageBox.Show("Please select a site owner to delete.");
                return;
            }

            _context.SiteOwners.Remove(SelectedSiteOwner);
            _context.SaveChanges();
            LoadSiteOwners();
            ClearFields();
        }

        private void ClearFields()
        {
            CompanyName = ContactPerson = Phone = Email = Address = string.Empty;
            SelectedSiteOwner = null;
        }
    }
}
