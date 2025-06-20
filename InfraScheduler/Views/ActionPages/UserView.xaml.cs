using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class UserView : UserControl
    {
        public UserView(UserViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
