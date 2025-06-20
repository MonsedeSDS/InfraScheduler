using System.Windows;
using System.Windows.Controls;
using InfraScheduler.ViewModels;

namespace InfraScheduler.Views.LandingViews
{
    /// <summary>
    /// Interaction logic for SiteManagementLandingView.xaml
    /// </summary>
    public partial class SiteManagementLandingView : UserControl
    {
        public SiteManagementLandingView()
        {
            InitializeComponent();
        }

        public SiteManagementLandingView(SiteManagementLandingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 