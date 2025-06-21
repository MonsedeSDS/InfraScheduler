using System.Windows.Controls;
using InfraScheduler.Delivery.ViewModels;

namespace InfraScheduler.Delivery.Views
{
    public partial class ProjectDetailView : UserControl
    {
        public ProjectDetailView(ProjectDetailViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 