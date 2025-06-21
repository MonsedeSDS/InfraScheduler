using InfraScheduler.ViewModels;
using System.Windows;

namespace InfraScheduler
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext will be set by App.xaml.cs
        }

        public MainWindow(NavigationViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}