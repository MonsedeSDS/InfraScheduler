using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class JobTaskView : UserControl
    {
        public JobTaskView(JobTaskViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
