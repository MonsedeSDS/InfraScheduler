using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
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
