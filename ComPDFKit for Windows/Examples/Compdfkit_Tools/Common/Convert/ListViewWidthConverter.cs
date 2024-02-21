using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Compdfkit_Tools.Common
{
    public class ListViewWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }
            else
            {
                if (value is double)
                {
                    double width = (double)value;
                    width = width - SystemParameters.VerticalScrollBarWidth - 8;
                    return Math.Max(0, width);
                }
                else
                {
                    return 0;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
