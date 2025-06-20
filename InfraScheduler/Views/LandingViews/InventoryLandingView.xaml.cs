using System.Windows;
using System.Windows.Controls;
using InfraScheduler.ViewModels;

namespace InfraScheduler.Views.LandingViews
{
    /// <summary>
    /// Interaction logic for EquipmentManagementLandingView.xaml
    /// </summary>
    public partial class EquipmentManagementLandingView : UserControl
    {
        public EquipmentManagementLandingView()
        {
            InitializeComponent();
        }

        public EquipmentManagementLandingView(EquipmentManagementLandingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 