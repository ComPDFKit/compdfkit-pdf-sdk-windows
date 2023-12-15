using ComPDFKitViewer.AnnotEvent;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Compdfkit_Tools.Common
{
    [ValueConversion(typeof(AnnotArgsType), typeof(Visibility))]
    public class AnnotArgsTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AnnotArgsType annotArgsType = (AnnotArgsType)value;
            if (annotArgsType.ToString() == parameter as string)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
