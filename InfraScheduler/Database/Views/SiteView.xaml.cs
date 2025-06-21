using System.Windows.Controls;
using InfraScheduler.Database.ViewModels;

namespace InfraScheduler.Database.Views
{
    public partial class SiteView : UserControl
    {
        public SiteView(SiteViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 