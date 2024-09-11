using ComPDFKit.Controls.Data;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ComPDFKit.Controls.Common
{
    [ValueConversion(typeof(CPDFAnnotationType), typeof(Visibility))]
    public class AnnotArgsTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CPDFAnnotationType annotArgsType = (CPDFAnnotationType)value;
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
