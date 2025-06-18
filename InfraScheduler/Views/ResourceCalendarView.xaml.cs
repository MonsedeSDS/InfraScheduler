using InfraScheduler.ViewModels;
using System.Windows.Controls;

namespace InfraScheduler.Views
{
    public partial class ResourceCalendarView : UserControl
    {
        public ResourceCalendarView(ResourceCalendarViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
