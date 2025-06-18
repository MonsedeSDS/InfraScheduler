using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class MaterialView : UserControl
    {
        public MaterialView(MaterialViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
