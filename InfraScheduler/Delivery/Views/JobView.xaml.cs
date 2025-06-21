using System.Windows.Controls;
using InfraScheduler.Delivery.ViewModels;

namespace InfraScheduler.Delivery.Views
{
    public partial class JobView : UserControl
    {
        public JobView(JobViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 