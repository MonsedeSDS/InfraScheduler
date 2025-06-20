using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class ClientView : UserControl
    {
        public ClientView(ClientViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}