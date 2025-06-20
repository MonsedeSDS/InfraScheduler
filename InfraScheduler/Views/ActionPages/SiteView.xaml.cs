using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class SiteView : UserControl
    {
        public SiteView(SiteViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
