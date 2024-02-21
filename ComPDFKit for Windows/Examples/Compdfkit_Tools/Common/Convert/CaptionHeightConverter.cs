using System;
using System.Globalization;
using System.Windows.Data;

namespace Compdfkit_Tools.Common
{
    public class CaptionHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (double)values[0] + (double)values[1];
            }
            catch
            {
                return 6.0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
