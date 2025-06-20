using System.Windows;
using System.Windows.Controls;
using InfraScheduler.ViewModels;

namespace InfraScheduler.Views.LandingViews
{
    /// <summary>
    /// Interaction logic for TechnicianManagementLandingView.xaml
    /// </summary>
    public partial class TechnicianManagementLandingView : UserControl
    {
        public TechnicianManagementLandingView()
        {
            InitializeComponent();
        }

        public TechnicianManagementLandingView(TechnicianManagementLandingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 