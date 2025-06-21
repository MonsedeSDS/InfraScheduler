using System.Windows.Controls;
using InfraScheduler.Inventory.ViewModels;

namespace InfraScheduler.Inventory.Views
{
    public partial class InventoryLandingView : UserControl
    {
        public InventoryLandingView(InventoryLandingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 