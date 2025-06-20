using System.Windows.Controls;
using InfraScheduler.Models.EquipmentManagement;
using InfraScheduler.ViewModels;
using InfraScheduler.Data;

namespace InfraScheduler.Views
{
    public partial class EquipmentView : UserControl
    {
        public EquipmentView(EquipmentViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
} 