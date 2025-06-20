using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class SiteOwnerView : UserControl
    {
        public SiteOwnerView(SiteOwnerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
