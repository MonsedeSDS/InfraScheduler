using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class SubcontractorViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private string _companyName = string.Empty;
        private string _contactPerson = string.Empty;
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private string _address = string.Empty;
        private string _notes = string.Empty;
        private Subcontractor? _selectedSubcontractor;
        private ObservableCollection<Subcontractor> _allSubcontractors = new();
        private ObservableCollection<Subcontractor> _filteredSubcontractors = new();

        public ObservableCollection<Subcontractor> Subcontractors
        {
            get => _filteredSubcontractors;
            set
            {
                _filteredSubcontractors = value;
                OnPropertyChanged();
            }
        }

        public string CompanyName
        {
            get => _companyName;
            set
            {
                if (_companyName != value)
                {
                    _companyName = value;
                    OnPropertyChanged();
                    FilterSubcontractors();
                }
            }
        }

        public string ContactPerson
        {
            get => _contactPerson;
            set
            {
                if (_contactPerson != value)
                {
                    _contactPerson = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
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

        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged();
                }
            }
        }

        public Subcontractor? SelectedSubcontractor
        {
            get => _selectedSubcontractor;
            set
            {
                if (_selectedSubcontractor != value)
                {
                    _selectedSubcontractor = value;
                    OnPropertyChanged();
                    if (_selectedSubcontractor != null)
                    {
                        LoadSubcontractorDetails(_selectedSubcontractor);
                    }
                }
            }
        }

        public SubcontractorViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");

            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _context = new InfraSchedulerContext(options);
            _context.Database.Migrate();

            LoadSubcontractors();
        }

        private void LoadSubcontractors()
        {
            _allSubcontractors.Clear();
            foreach (var subcontractor in _context.Subcontractors.ToList())
            {
                _allSubcontractors.Add(subcontractor);
            }
            FilterSubcontractors();
        }

        private void FilterSubcontractors()
        {
            var filtered = string.IsNullOrWhiteSpace(CompanyName)
                ? _allSubcontractors
                : _allSubcontractors.Where(s => s.CompanyName.Contains(CompanyName, StringComparison.OrdinalIgnoreCase));

            Subcontractors = new ObservableCollection<Subcontractor>(filtered);
        }

        private void LoadSubcontractorDetails(Subcontractor subcontractor)
        {
            CompanyName = subcontractor.CompanyName;
            ContactPerson = subcontractor.ContactPerson;
            Phone = subcontractor.Phone;
            Email = subcontractor.Email;
            Address = subcontractor.Address;
            Notes = subcontractor.Notes ?? string.Empty;
        }

        [RelayCommand]
        private void AddSubcontractor()
        {
            var newSubcontractor = new Subcontractor
            {
                CompanyName = CompanyName,
                ContactPerson = ContactPerson,
                Phone = Phone,
                Email = Email,
                Address = Address,
                Notes = Notes
            };

            _context.Subcontractors.Add(newSubcontractor);
            _context.SaveChanges();
            LoadSubcontractors();
            ClearFields();
        }

        [RelayCommand]
        private void UpdateSubcontractor()
        {
            if (SelectedSubcontractor == null)
            {
                MessageBox.Show("Please select a subcontractor to update.");
                return;
            }

            SelectedSubcontractor.CompanyName = CompanyName;
            SelectedSubcontractor.ContactPerson = ContactPerson;
            SelectedSubcontractor.Phone = Phone;
            SelectedSubcontractor.Email = Email;
            SelectedSubcontractor.Address = Address;
            SelectedSubcontractor.Notes = Notes;

            _context.SaveChanges();
            LoadSubcontractors();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteSubcontractor()
        {
            if (SelectedSubcontractor == null)
            {
                MessageBox.Show("Please select a subcontractor to delete.");
                return;
            }

            _context.Subcontractors.Remove(SelectedSubcontractor);
            _context.SaveChanges();
            LoadSubcontractors();
            ClearFields();
        }

        private void ClearFields()
        {
            CompanyName = ContactPerson = Phone = Email = Address = Notes = string.Empty;
            SelectedSubcontractor = null;
        }
    }
}
