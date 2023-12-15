using System;
using System.Globalization;
using System.Windows.Data;

namespace Compdfkit_Tools.Common
{
    public class TextLengthToBoolConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            else
            {
                if (value is string)
                {
                    string checkStr = (string)value;
                    if (string.IsNullOrEmpty(checkStr) == false )
                    {
                       return true;
                    }
                }
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
