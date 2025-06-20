using System.Windows;
using System.Windows.Controls;
using InfraScheduler.ViewModels;

namespace InfraScheduler.Views.LandingViews
{
    /// <summary>
    /// Interaction logic for MainLandingView.xaml
    /// </summary>
    public partial class MainLandingView : UserControl
    {
        public MainLandingView()
        {
            InitializeComponent();
        }

        public MainLandingView(MainLandingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 