using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class SubcontractorView : UserControl
    {
        public SubcontractorView(SubcontractorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
