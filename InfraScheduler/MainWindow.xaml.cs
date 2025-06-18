using InfraScheduler.ViewModels;
using System.Windows;

namespace InfraScheduler
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new NavigationViewModel();
        }

        public MainWindow(NavigationViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}