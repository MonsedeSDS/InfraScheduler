using System.Windows.Controls;
using InfraScheduler.Database.ViewModels;

namespace InfraScheduler.Database.Views
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