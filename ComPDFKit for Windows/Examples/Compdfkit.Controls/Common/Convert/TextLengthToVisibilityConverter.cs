using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ComPDFKit.Controls.Common
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class TextLengthToVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }
            else
            {
                if (value is string)
                {
                    string  checkStr = (string)value;
                    if(string.IsNullOrEmpty(checkStr)==false)
                    {
                        return Visibility.Collapsed;
                    }
                }
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    [ValueConversion(typeof(string), typeof(Visibility))]
    internal class InvertTextLengthToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }
            else
            {
                if (value is string)
                {
                    string checkStr = (string)value;
                    if (string.IsNullOrEmpty(checkStr) == false)
                    {
                        return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
