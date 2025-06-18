using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class MaterialResourceView : UserControl
    {
        public MaterialResourceView(MaterialResourceViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
