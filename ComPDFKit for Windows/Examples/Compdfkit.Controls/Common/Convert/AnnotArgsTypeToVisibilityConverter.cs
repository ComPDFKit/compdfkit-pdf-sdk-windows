using ComPDFKit.PDFAnnotation;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ComPDFKit.Controls.Common
{
    [ValueConversion(typeof(C_ANNOTATION_TYPE), typeof(Visibility))]
    public class AnnotArgsTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            C_ANNOTATION_TYPE annotArgsType = (C_ANNOTATION_TYPE)value;
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
