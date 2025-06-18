using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class TaskDependencyView : UserControl
    {
        public TaskDependencyView(TaskDependencyViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
