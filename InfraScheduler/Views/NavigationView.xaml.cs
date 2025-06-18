using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class NavigationView : UserControl
    {
        public NavigationView(NavigationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 