using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFSearchInputUI : UserControl
    {
        public enum MoveDirection
        {
            Previous = 0,
            Next = 1
        };
    
        public event EventHandler<string> SearchEvent;

        public event EventHandler ClearEvent;
        
        public event EventHandler<MoveDirection> MoveResultEvent;

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
        
        public string ReplaceWord
        {
            get
            {
                return ReplaceTextBox.Text;
            }
            set
            {
                ReplaceTextBox.Text = value;
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
                ReplaceTextBox.Width = value;
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
                ReplaceTextBox.Height = value;
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

        private void ReplaceTextClear_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ReplaceTextBox.Text = string.Empty;
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            MoveResultEvent?.Invoke(this,MoveDirection.Previous);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            MoveResultEvent?.Invoke(this,MoveDirection.Next);
        }
    }

}
