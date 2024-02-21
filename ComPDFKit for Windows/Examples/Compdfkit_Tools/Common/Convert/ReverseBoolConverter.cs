using System;
using System.Windows.Data;
using System.Globalization;

namespace Compdfkit_Tools.Common
{

    [ValueConversion(typeof(bool), typeof(bool))]
    public class ReverseBoolConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (bool)value != true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }

            return value;
        }
    }
}
