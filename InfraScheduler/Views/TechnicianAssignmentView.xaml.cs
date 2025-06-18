using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class TechnicianAssignmentView : UserControl
    {
        public TechnicianAssignmentView(TechnicianAssignmentViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
