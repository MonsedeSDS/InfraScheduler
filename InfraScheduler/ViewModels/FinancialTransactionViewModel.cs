using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfraScheduler.Data;
using InfraScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace InfraScheduler.ViewModels
{
    public partial class FinancialTransactionViewModel : ObservableObject
    {
        private readonly InfraSchedulerContext _context;

        [ObservableProperty] private DateTime transactionDate = DateTime.Now;
        [ObservableProperty] private string description = string.Empty;
        [ObservableProperty] private decimal amount;
        [ObservableProperty] private string transactionType = string.Empty;
        [ObservableProperty] private int jobId;
        [ObservableProperty] private FinancialTransaction? selectedTransaction;

        public ObservableCollection<FinancialTransaction> FinancialTransactions { get; set; } = new();

        public FinancialTransactionViewModel()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfraScheduler.db");

            var options = new DbContextOptionsBuilder<InfraSchedulerContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _context = new InfraSchedulerContext(options);

            // THE MISSING PART:
            _context.Database.Migrate();

            LoadTransactions();
        }


        private void LoadTransactions()
        {
            FinancialTransactions.Clear();
            foreach (var transaction in _context.FinancialTransactions.ToList())
            {
                FinancialTransactions.Add(transaction);
            }
        }

        [RelayCommand]
        private void AddTransaction()
        {
            var newTransaction = new FinancialTransaction
            {
                TransactionDate = TransactionDate,
                Description = Description,
                Amount = Amount,
                TransactionType = TransactionType,
                JobId = JobId
            };

            _context.FinancialTransactions.Add(newTransaction);
            _context.SaveChanges();
            LoadTransactions();
            ClearFields();
        }

        [RelayCommand]
        private void UpdateTransaction()
        {
            if (SelectedTransaction == null)
            {
                MessageBox.Show("Please select a transaction to update.");
                return;
            }

            SelectedTransaction.TransactionDate = TransactionDate;
            SelectedTransaction.Description = Description;
            SelectedTransaction.Amount = Amount;
            SelectedTransaction.TransactionType = TransactionType;
            SelectedTransaction.JobId = JobId;

            _context.SaveChanges();
            LoadTransactions();
            ClearFields();
        }

        [RelayCommand]
        private void DeleteTransaction()
        {
            if (SelectedTransaction == null)
            {
                MessageBox.Show("Please select a transaction to delete.");
                return;
            }

            _context.FinancialTransactions.Remove(SelectedTransaction);
            _context.SaveChanges();
            LoadTransactions();
            ClearFields();
        }

        private void ClearFields()
        {
            TransactionDate = DateTime.Now;
            Description = string.Empty;
            Amount = 0;
            TransactionType = string.Empty;
            JobId = 0;
            SelectedTransaction = null;
        }
    }
}
