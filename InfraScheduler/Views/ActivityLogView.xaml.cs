using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class ActivityLogView : UserControl
    {
        public ActivityLogView(ActivityLogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}