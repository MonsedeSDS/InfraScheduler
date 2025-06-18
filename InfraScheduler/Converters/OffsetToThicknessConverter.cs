using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InfraScheduler.Converters
{
    public class OffsetToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double offset)
            {
                return new Thickness(offset, 0, 0, 0);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Thickness thickness)
            {
                return thickness.Left;
            }
            return 0.0;
        }
    }
}
