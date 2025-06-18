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

namespace InfraScheduler.ViewModels
{
    public partial class ClientViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;
        private string _name = string.Empty;
        private string _contactPerson = string.Empty;
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private string _address = string.Empty;
        private string _notes = string.Empty;
        private string _searchTerm = string.Empty;
        private Client? _selectedClient;
        private bool _isLoading;
        private ObservableCollection<Client> _clients = new();

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                OnPropertyChanged(nameof(Clients));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
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

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    OnPropertyChanged();
                    _ = LoadClientsAsync();
                }
            }
        }

        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (_selectedClient != value)
                {
                    _selectedClient = value;
                    OnPropertyChanged();
                    if (_selectedClient != null)
                    {
                        LoadClientDetails(_selectedClient);
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
        public ICommand AddClientCommand { get; }
        public ICommand UpdateClientCommand { get; }
        public ICommand DeleteClientCommand { get; }
        public ICommand ClearCommand { get; }

        public ClientViewModel(InfraSchedulerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            
            LoadDataCommand = new AsyncRelayCommand(LoadClientsAsync);
            AddClientCommand = new AsyncRelayCommand(AddClientAsync);
            UpdateClientCommand = new AsyncRelayCommand(UpdateClientAsync);
            DeleteClientCommand = new AsyncRelayCommand(DeleteClientAsync);
            ClearCommand = new AsyncRelayCommand(ClearFields);

            _ = LoadClientsAsync();
        }

        private async Task LoadClientsAsync()
        {
            try
            {
                IsLoading = true;
                var query = _context.Clients.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    query = query.Where(c => 
                        c.Name.Contains(SearchTerm) || 
                        c.ContactPerson.Contains(SearchTerm) || 
                        c.Email.Contains(SearchTerm) ||
                        c.Phone.Contains(SearchTerm));
                }

                var clients = await query.ToListAsync();
                Clients = new ObservableCollection<Client>(clients);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading clients: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadClientDetails(Client client)
        {
            Name = client.Name;
            ContactPerson = client.ContactPerson;
            Phone = client.Phone;
            Email = client.Email;
            Address = client.Address;
            Notes = client.Notes ?? string.Empty;
        }

        private async Task AddClientAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Client name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                var client = new Client
                {
                    Name = Name,
                    ContactPerson = ContactPerson,
                    Phone = Phone,
                    Email = Email,
                    Address = Address,
                    Notes = Notes,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
                await LoadClientsAsync();
                ClearFields();
                MessageBox.Show("Client added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateClientAsync()
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("Please select a client to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Client name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                SelectedClient.Name = Name;
                SelectedClient.ContactPerson = ContactPerson;
                SelectedClient.Phone = Phone;
                SelectedClient.Email = Email;
                SelectedClient.Address = Address;
                SelectedClient.Notes = Notes;
                SelectedClient.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                await LoadClientsAsync();
                MessageBox.Show("Client updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteClientAsync()
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("Please select a client to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to delete this client?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    _context.Clients.Remove(SelectedClient);
                    await _context.SaveChangesAsync();
                    await LoadClientsAsync();
                    ClearFields();
                    MessageBox.Show("Client deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ClearFields()
        {
            Name = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            Notes = string.Empty;
            SelectedClient = null;
            await Task.CompletedTask;
        }
    }
}
