using System.Windows;
using System.Windows.Controls;
using InfraScheduler.ViewModels;

namespace InfraScheduler.Views.LandingViews
{
    /// <summary>
    /// Interaction logic for JobManagementLandingView.xaml
    /// </summary>
    public partial class JobManagementLandingView : UserControl
    {
        public JobManagementLandingView()
        {
            InitializeComponent();
        }

        public JobManagementLandingView(JobManagementLandingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 