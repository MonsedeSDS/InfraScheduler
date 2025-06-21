using System.Windows.Controls;
using System.Windows.Input;

namespace InfraScheduler.Core.Views
{
    public partial class MainLandingView : UserControl
    {
        public MainLandingView()
        {
            InitializeComponent();
        }

        private void DeliveryGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is Core.ViewModels.MainLandingViewModel viewModel)
            {
                viewModel.NavigateToDeliveryCommand.Execute(null);
            }
        }

        private void InventoryGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is Core.ViewModels.MainLandingViewModel viewModel)
            {
                viewModel.NavigateToInventoryCommand.Execute(null);
            }
        }

        private void DatabaseGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is Core.ViewModels.MainLandingViewModel viewModel)
            {
                viewModel.NavigateToDatabaseCommand.Execute(null);
            }
        }
    }
} 