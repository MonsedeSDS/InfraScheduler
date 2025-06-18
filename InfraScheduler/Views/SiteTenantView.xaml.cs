using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class SiteTenantView : UserControl
    {
        public SiteTenantView(SiteTenantViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
