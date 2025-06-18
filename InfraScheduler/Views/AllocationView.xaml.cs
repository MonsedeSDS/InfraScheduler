using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class AllocationView : UserControl
    {
        public AllocationView(AllocationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}