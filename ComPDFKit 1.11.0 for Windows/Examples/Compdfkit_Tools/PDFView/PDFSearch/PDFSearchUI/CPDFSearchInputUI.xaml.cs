using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFSearchInputUI : UserControl
    {
        public event EventHandler<string> SearchEvent;

        public event EventHandler ClearEvent;

        public string SearchKeyWord
        {
            get
            {
                return SearchTextBox.Text;
            }
            set
            {
                SearchTextBox.Text = value;
            }
        }

        public double InputTextBoxWidth
        {
            get
            {
                return SearchTextBox.Width;
            }
            set
            {
                SearchTextBox.Width = value;
            }
        }

        public double InputTextBoxHeight
        {
            get
            {
                return SearchTextBox.Height;
            }
            set
            {
                SearchTextBox.Height = value;
            }
        }

        public CPDFSearchInputUI()
        {
            InitializeComponent();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && string.IsNullOrEmpty(SearchKeyWord)==false)
            {
                SearchEvent?.Invoke(this, SearchKeyWord);
            }
        }

        private void TextClear_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SearchKeyWord = string.Empty;
            ClearEvent?.Invoke(this,new EventArgs());
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchKeyWord) == false)
            {
                SearchEvent?.Invoke(this, SearchKeyWord);
            }
        }
    }

}
