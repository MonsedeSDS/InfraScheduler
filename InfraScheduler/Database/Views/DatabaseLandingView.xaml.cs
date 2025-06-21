using System.Windows.Controls;
using InfraScheduler.Database.ViewModels;

namespace InfraScheduler.Database.Views
{
    public partial class DatabaseLandingView : UserControl
    {
        public DatabaseLandingView(DatabaseLandingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 