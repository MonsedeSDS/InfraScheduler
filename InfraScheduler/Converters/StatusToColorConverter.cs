using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InfraScheduler.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "completed" => new SolidColorBrush(Colors.Green),
                    "in progress" => new SolidColorBrush(Colors.Orange),
                    "not started" => new SolidColorBrush(Colors.Gray),
                    "delayed" => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Black)
                };
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}