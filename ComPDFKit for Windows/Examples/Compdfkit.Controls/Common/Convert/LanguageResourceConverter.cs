using System;
using System.Globalization;
using System.Resources;
using System.Windows.Data;
using ComPDFKit.Controls.Helper;

namespace ComPDFKit.Controls.Common
{
    public class BotaResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                return string.Empty;
            }

            return LanguageHelper.BotaManager.GetString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CommonResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                return string.Empty;
            }

            return LanguageHelper.CommonManager.GetString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class PropertyPanelResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                return string.Empty;
            }

            return LanguageHelper.PropertyPanelManager.GetString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ToolBarResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                return string.Empty;
            }

            return LanguageHelper.ToolBarManager.GetString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class SigResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                return string.Empty;
            }

            return LanguageHelper.SigManager.GetString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    
    public class DocInfoResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                return string.Empty;
            }

            return LanguageHelper.DocInfoManager.GetString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    
    public class SecurityResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                return string.Empty;
            }

            return LanguageHelper.SecurityManager.GetString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    
    public class DocEditorResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                return string.Empty;
            }

            return LanguageHelper.DocEditorManager.GetString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    
    public class CompareResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                return string.Empty;
            }

            return LanguageHelper.CompareManager.GetString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}