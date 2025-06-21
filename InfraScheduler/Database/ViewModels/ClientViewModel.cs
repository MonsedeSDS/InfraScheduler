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
    public partial class ClientViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _phone = string.Empty;
        [ObservableProperty] private string _company = string.Empty;
        [ObservableProperty] private string _address = string.Empty;
        [ObservableProperty] private Client? _selectedClient;

        [ObservableProperty] private ObservableCollection<Client> _clients = new();

        public ClientViewModel(InfraSchedulerContext context)
        {
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            var clients = _context.Clients.ToList();

            Clients.Clear();
            foreach (var client in clients)
            {
                Clients.Add(client);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                if (SelectedClient == null)
                {
                    // Create new client
                    var client = new Client
                    {
                        Name = Name,
                        Email = Email,
                        Phone = Phone,
                        Address = Address
                    };

                    _context.Clients.Add(client);
                }
                else
                {
                    // Update existing client
                    SelectedClient.Name = Name;
                    SelectedClient.Email = Email;
                    SelectedClient.Phone = Phone;
                    SelectedClient.Name = Name;
                    SelectedClient.Address = Address;
                }

                await _context.SaveChangesAsync();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving client: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Update()
        {
            await Save();
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedClient == null) return;

            try
            {
                _context.Clients.Remove(SelectedClient);
                await _context.SaveChangesAsync();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting client: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            Company = string.Empty;
            Address = string.Empty;
            SelectedClient = null;
        }

        partial void OnSelectedClientChanged(Client? value)
        {
            if (value != null)
            {
                Name = value.Name;
                Email = value.Email;
                Phone = value.Phone;
                Company = value.Name;
                Address = value.Address;
            }
        }
    }
} 