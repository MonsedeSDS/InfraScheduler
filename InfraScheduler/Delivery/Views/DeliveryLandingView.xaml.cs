using System.Windows.Controls;
using InfraScheduler.Delivery.ViewModels;

namespace InfraScheduler.Delivery.Views
{
    public partial class DeliveryLandingView : UserControl
    {
        public DeliveryLandingView(DeliveryLandingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 