using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class FinancialTransactionView : UserControl
    {
        public FinancialTransactionView(FinancialTransactionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}