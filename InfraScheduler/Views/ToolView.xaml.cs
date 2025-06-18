using System.Windows.Controls;
using InfraScheduler.ViewModels;

namespace InfraScheduler.Views
{
    public partial class ToolView : UserControl
    {
        public ToolView()
        {
            InitializeComponent();
            DataContext = new ToolViewModel();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
} 