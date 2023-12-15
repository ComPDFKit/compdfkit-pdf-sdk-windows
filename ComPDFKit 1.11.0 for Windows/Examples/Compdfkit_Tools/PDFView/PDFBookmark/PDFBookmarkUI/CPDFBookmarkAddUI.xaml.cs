using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFBookmarkAddUI : UserControl
    {
        public event EventHandler<BookmarkChangeData> BookmarkAddEvent;
 
        public event EventHandler BookmarkInputExpandEvent;

        private bool toggleState;

        private BookmarkChangeData bookmarkData=new BookmarkChangeData();
        public CPDFBookmarkAddUI()
        {
            InitializeComponent();
        }
 
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            toggleState = !toggleState;
            BookmarkInputPanel.Visibility=toggleState?Visibility.Visible:Visibility.Collapsed;
            if(toggleState)
            {
                BookmarkInputExpandEvent?.Invoke(this, EventArgs.Empty);
            }
           
        }
 
        private void ButtonCancel_Click(object sender, MouseButtonEventArgs e)
        {
            HideInputUI(true);
        }
 
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(BookmarkText.Text) && bookmarkData!=null)
            {
                bookmarkData.NewTitle = BookmarkText.Text;
                BookmarkAddEvent?.Invoke(this, bookmarkData);
                bookmarkData = null;
                BookmarkText.Text = string.Empty;
                HideInputUI(true);
            }
        }

        public void SetBookmarkChangeData(BookmarkChangeData newChangeData)
        {
            if(newChangeData!=null)
            {
                PageNumText.Text = LanguageHelper.BotaManager.GetString("Text_Page") + " " + (newChangeData.PageIndex + 1);
                BookmarkText.Text = newChangeData.BookmarkTitle;
                BookmarkText.Focus();
                BookmarkText.SelectAll();
            }
            bookmarkData=newChangeData;
        }
        public void HideInputUI(bool isHide)
        {
            toggleState = !isHide;
            BookmarkInputPanel.Visibility = isHide? Visibility.Collapsed:Visibility.Visible;
        }

        private void DeleteBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BookmarkText.Text = string.Empty;
        }
    }

    internal class BoolEnableConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            else
            {
                try
                {
                    if (value is string)
                    {
                       string checkValue=value as string;
                        if(checkValue.Length>0)
                        {
                            return true;
                        }
                    }
                   
                }
                catch(Exception ex)
                {

                }
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
