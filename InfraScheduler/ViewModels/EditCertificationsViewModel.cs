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
    public partial class EditCertificationsViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private readonly Window _window;
        private readonly ObservableCollection<string> _originalCertifications;

        [ObservableProperty]
        private ObservableCollection<string> selectedCertifications;

        [ObservableProperty]
        private ObservableCollection<string> allCertifications;

        [ObservableProperty]
        private string newCertification = string.Empty;

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public EditCertificationsViewModel(InfraSchedulerContext context, Window window, ObservableCollection<string> certifications)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _originalCertifications = certifications;
            SelectedCertifications = new ObservableCollection<string>(certifications);
            LoadAllCertifications();
        }

        private async void LoadAllCertifications()
        {
            try
            {
                var certs = await _context.Certifications
                    .OrderBy(c => c.Name)
                    .Select(c => c.Name)
                    .ToListAsync();
                
                AllCertifications = new ObservableCollection<string>(certs);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading certifications: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void RemoveCertification(string certification)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(certification))
                {
                    MessageBox.Show("Please select a certification to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to remove {certification}?",
                    "Confirm Remove",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SelectedCertifications.Remove(certification);
                    AllCertifications.Remove(certification);
                    
                    // Remove from database
                    var certToRemove = _context.Certifications.FirstOrDefault(c => c.Name == certification);
                    if (certToRemove != null)
                    {
                        _context.Certifications.Remove(certToRemove);
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error removing certification: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddNewCertification()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewCertification))
                {
                    MessageBox.Show("Please enter a certification name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var trimmedCert = NewCertification.Trim();
                if (trimmedCert.Length > 100)
                {
                    MessageBox.Show("Certification name is too long (maximum 100 characters).", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedCertifications.Contains(trimmedCert))
                {
                    MessageBox.Show("This certification is already added.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.Certifications.Add(new Certification { Name = trimmedCert });
                await _context.SaveChangesAsync();

                // Refresh the list from database
                var certs = await _context.Certifications
                    .OrderBy(c => c.Name)
                    .Select(c => c.Name)
                    .ToListAsync();
                
                AllCertifications = new ObservableCollection<string>(certs);
                NewCertification = string.Empty;

                MessageBox.Show("New certification added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding new certification: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Save()
        {
            _originalCertifications.Clear();
            foreach (var cert in SelectedCertifications)
            {
                _originalCertifications.Add(cert);
            }
            _window.DialogResult = true;
            _window.Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
} 