using System.Windows.Controls;
using InfraScheduler.ViewModels;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

namespace InfraScheduler.Views
{
    public partial class GanttView : UserControl
    {
        private const double PixelsPerDay = 50; // Base scale
        private DateTime _startDate;
        private DateTime _endDate;
        private double _zoomLevel = 1.0;

        public GanttView()
        {
            InitializeComponent();
            DataContext = new GanttViewModel();
            Loaded += GanttView_Loaded;
            TimeScaleCanvas.SizeChanged += TimeScaleCanvas_SizeChanged;
        }

        private void GanttView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GanttViewModel viewModel)
            {
                var allTasks = viewModel.TaskGroups.SelectMany(g => g.Tasks).ToList();
                if (!allTasks.Any())
                {
                    // If no tasks, use default date range
                    _startDate = DateTime.Today;
                    _endDate = DateTime.Today.AddDays(30);
                }
                else
                {
                    _startDate = allTasks.Min(t => t.StartDate);
                    _endDate = allTasks.Max(t => t.EndDate);
                }
                DrawTimeScale();
                DrawTaskBars();
            }
        }

        private void TimeScaleCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawTimeScale();
            DrawTaskBars();
        }

        private void DrawTimeScale()
        {
            TimeScaleCanvas.Children.Clear();

            var currentDate = _startDate;
            var x = 0.0;
            var dayWidth = PixelsPerDay * _zoomLevel;

            while (currentDate <= _endDate)
            {
                // Draw vertical line
                var line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = 30,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1
                };
                TimeScaleCanvas.Children.Add(line);

                // Draw date label
                var textBlock = new TextBlock
                {
                    Text = currentDate.ToString("MM/dd"),
                    Margin = new Thickness(x + 2, 2, 0, 0)
                };
                TimeScaleCanvas.Children.Add(textBlock);

                currentDate = currentDate.AddDays(1);
                x += dayWidth;
            }
        }

        private void DrawTaskBars()
        {
            if (DataContext is not GanttViewModel viewModel) return;

            var taskBars = new System.Collections.Generic.List<object>();
            var y = 0.0;
            var dayWidth = PixelsPerDay * _zoomLevel;

            foreach (var group in viewModel.TaskGroups)
            {
                if (!group.IsExpanded) continue;

                foreach (var task in group.Tasks)
                {
                    var startOffset = (task.StartDate - _startDate).TotalDays * dayWidth;
                    var duration = (task.EndDate - task.StartDate).TotalDays * dayWidth;
                    var progressWidth = duration * task.Progress;

                    taskBars.Add(new
                    {
                        Name = task.Name,
                        Status = task.Status,
                        Progress = task.Progress,
                        Width = duration,
                        Left = startOffset,
                        Top = y,
                        ProgressWidth = progressWidth,
                        Tooltip = task.Tooltip,
                        Dependencies = task.Dependencies.Select(d => new
                        {
                            PathData = CalculateDependencyPath(d, y, dayWidth)
                        })
                    });

                    y += 30; // Height of task bar + margin
                }
            }

            TaskBarsControl.ItemsSource = taskBars;
        }

        private string CalculateDependencyPath(Dependency dependency, double y, double dayWidth)
        {
            if (DataContext is not GanttViewModel viewModel) return string.Empty;

            // Find the connected tasks
            var fromTask = viewModel.TaskGroups
                .SelectMany(g => g.Tasks)
                .FirstOrDefault(t => t.Id == dependency.FromTaskId);
            var toTask = viewModel.TaskGroups
                .SelectMany(g => g.Tasks)
                .FirstOrDefault(t => t.Id == dependency.ToTaskId);

            if (fromTask == null || toTask == null) return string.Empty;

            // Calculate positions
            var startX = (fromTask.EndDate - _startDate).TotalDays * dayWidth;
            var endX = (toTask.StartDate - _startDate).TotalDays * dayWidth;
            var midY = y - 20; // Arrow goes up and then down

            return $"M {startX},{y} L {startX},{midY} L {endX},{midY} L {endX},{y}";
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            _zoomLevel = Math.Min(_zoomLevel * 1.2, 3.0);
            DrawTimeScale();
            DrawTaskBars();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _zoomLevel = Math.Max(_zoomLevel / 1.2, 0.5);
            DrawTimeScale();
            DrawTaskBars();
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            if (today < _startDate || today > _endDate)
            {
                _startDate = today.AddDays(-7);
                _endDate = today.AddDays(7);
                DrawTimeScale();
                DrawTaskBars();
            }

            // Scroll to today
            var scrollViewer = FindVisualChild<ScrollViewer>(this);
            if (scrollViewer != null)
            {
                var todayOffset = (today - _startDate).TotalDays * PixelsPerDay * _zoomLevel;
                scrollViewer.ScrollToHorizontalOffset(todayOffset);
            }
        }

        private void Fit_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not GanttViewModel viewModel) return;

            var scrollViewer = FindVisualChild<ScrollViewer>(this);
            if (scrollViewer == null) return;

            var availableWidth = scrollViewer.ViewportWidth;
            var totalDays = (_endDate - _startDate).TotalDays;
            _zoomLevel = availableWidth / (totalDays * PixelsPerDay);

            DrawTimeScale();
            DrawTaskBars();
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }
    }
}