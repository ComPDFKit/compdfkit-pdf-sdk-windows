using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Drawing;
using System.Globalization;
using System.Windows.Media;

namespace Compdfkit_Tools.Common
{
    /// <summary>
    /// Value converter between bool and Brushes
    /// </summary>
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (boolValue)
                {
                    return new SolidColorBrush(Colors.Black);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }
            }
            return new SolidColorBrush(Colors.Black);; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); 
        }
    }
}
