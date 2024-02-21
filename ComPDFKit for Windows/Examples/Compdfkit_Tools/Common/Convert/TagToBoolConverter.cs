using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Compdfkit_Tools.Common
{
    public class TagToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int v && parameter is string v1)
            {
                if (int.TryParse(v1, out int parameterValue))
                {
                    return v == parameterValue;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && parameter is int && (bool)value)
            {
                return (int)parameter;
            }
            return Binding.DoNothing;
        }
    }
}
