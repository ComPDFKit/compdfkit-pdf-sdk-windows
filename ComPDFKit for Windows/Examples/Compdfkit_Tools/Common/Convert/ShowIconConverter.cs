using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Compdfkit_Tools.Common
{
    public class ShowIconConverter: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var Visible = ((values[1] == DependencyProperty.UnsetValue ? 0 : (int)values[1]) >=
                (values[0] == DependencyProperty.UnsetValue ? 0 : (int)values[0]) ? Visibility.Collapsed : Visibility.Visible );
            return Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
