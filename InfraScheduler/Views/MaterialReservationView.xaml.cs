using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class MaterialReservationView : UserControl
    {
        public MaterialReservationView(MaterialReservationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}