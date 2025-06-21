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
using System.Timers;

namespace InfraScheduler.Database.ViewModels
{
    public partial class SubcontractorViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly System.Timers.Timer _searchTimer;
        private ObservableCollection<Subcontractor> _allSubcontractors;

        [ObservableProperty]
        private ObservableCollection<Subcontractor> subcontractors;

        [ObservableProperty]
        private string searchTerm = string.Empty;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string phone = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string specialization = string.Empty;

        [ObservableProperty]
        private Subcontractor? selectedSubcontractor;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public SubcontractorViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _allSubcontractors = new ObservableCollection<Subcontractor>();
            Subcontractors = new ObservableCollection<Subcontractor>();

            // Initialize search timer
            _searchTimer = new System.Timers.Timer(300); // 300ms delay
            _searchTimer.Elapsed += (s, e) => 
            {
                _searchTimer.Stop();
                Application.Current.Dispatcher.Invoke(() => FilterSubcontractors());
            };

            LoadSubcontractors();
        }

        private async Task LoadSubcontractors()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _allSubcontractors.Clear();
                var subcontractors = await _context.Subcontractors.ToListAsync();
                foreach (var subcontractor in subcontractors)
                {
                    _allSubcontractors.Add(subcontractor);
                }
                FilterSubcontractors();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading subcontractors: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSearchTermChanged(string value)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void FilterSubcontractors()
        {
            Subcontractors.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchTerm)
                ? _allSubcontractors
                : _allSubcontractors.Where(s =>
                    (s.CompanyName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)); 
            foreach (var subcontractor in filtered)
            {
                Subcontractors.Add(subcontractor);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateSubcontractorData()) return;

            try
            {
                var subcontractor = new Subcontractor
                {
                    CompanyName = Name,
                    Phone = Phone,
                    Email = Email
                };

                _context.Subcontractors.Add(subcontractor);
                await _context.SaveChangesAsync();
                await LoadSubcontractors();
                ClearFields();
                MessageBox.Show("Subcontractor added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving subcontractor: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Update()
        {
            if (SelectedSubcontractor == null || !ValidateSubcontractorData()) return;

            try
            {
                SelectedSubcontractor.CompanyName = Name;
                SelectedSubcontractor.Phone = Phone;
                SelectedSubcontractor.Email = Email;
                // Specialization property doesn't exist in Subcontractor model - removing

                await _context.SaveChangesAsync();
                await LoadSubcontractors();
                ClearFields();
                MessageBox.Show("Subcontractor updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating subcontractor: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedSubcontractor == null) return;

                            var result = MessageBox.Show($"Are you sure you want to delete {SelectedSubcontractor.CompanyName}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Subcontractors.Remove(SelectedSubcontractor);
                    await _context.SaveChangesAsync();
                    await LoadSubcontractors();
                    ClearFields();
                    MessageBox.Show("Subcontractor deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error deleting subcontractor: {ex.Message}";
                    MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateSubcontractorData()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Company name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearFields()
        {
            Name = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            Specialization = string.Empty;
            SelectedSubcontractor = null;
        }

        partial void OnSelectedSubcontractorChanged(Subcontractor? value)
        {
            if (value != null)
            {
                Name = value.CompanyName ?? string.Empty;
                Phone = value.Phone ?? string.Empty;
                Email = value.Email ?? string.Empty;
                // Removed Specialization assignment - property doesn't exist
            }
        }
    }
} 