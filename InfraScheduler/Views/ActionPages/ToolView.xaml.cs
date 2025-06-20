using System.Windows.Controls;
using InfraScheduler.ViewModels;

namespace InfraScheduler.Views
{
    public partial class ToolView : UserControl
    {
        public ToolView(ToolViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
} 