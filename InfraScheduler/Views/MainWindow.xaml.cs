using InfraScheduler.ViewModels;
using System.Windows;
using System.Diagnostics;

namespace InfraScheduler.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(NavigationViewModel navigationViewModel)
        {
            InitializeComponent();
            DataContext = navigationViewModel;
        }

        private void RunTests_Click(object sender, RoutedEventArgs e)
        {
            RunTests();
        }

        private void RunTests()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "test",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                MessageBox.Show(output + error);
            }
        }
    }
} 