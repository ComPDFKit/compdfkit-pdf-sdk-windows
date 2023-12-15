using System;
using System.Globalization;
using System.Windows.Data;

namespace Compdfkit_Tools.Common
{
    public class IntAndTagToBoolMultiBinding : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null)
            {
                return false;
            }
            else
            {
                if (values[0].ToString() == values[1].ToString())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToBoolean(value))
            {
                string Tag = parameter.ToString();
                int index = System.Convert.ToInt32(parameter);
                return new object[] { Tag, index };
            }
            return null;
        }
    }
}
