using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Compdfkit_Tools.Common
{
    public class ReverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
            {
                return Visibility.Collapsed;
            }
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
            {
                return Visibility.Collapsed;
            }
            else
                return Visibility.Visible;
        }
    }
}
