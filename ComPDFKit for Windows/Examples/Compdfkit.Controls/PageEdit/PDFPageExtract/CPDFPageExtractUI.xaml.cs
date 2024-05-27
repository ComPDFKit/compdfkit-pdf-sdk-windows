using System;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControlUI
{
    public partial class CPDFPageExtractUI : UserControl
    {
        public int MaxiumIndex
        {
            get;
            set;
        }

        private int _maxIndex;
        public int MaxIndex
        {
            get => _maxIndex;
            set
            {
                _maxIndex = value;
                MaxPageTextBlock.Text = _maxIndex.ToString();
            }
        }

        public string CurrentPageRange
        {
            set => CustomPageRangeTextBox.Text = value;
        }

        public PageRange PageRange
        {
            set
            {
                if (value == PageRange.AllPages)
                {
                    AllPagesRadioButton.IsChecked = true;
                }
                else if (value == PageRange.OddPages)
                {
                    OddPagesRadioButton.IsChecked = true;
                }
                else if (value == PageRange.EvenPages)
                {
                    EvenPagesRadioButton.IsChecked = true;
                }
                else
                {
                    CustomPagesRadioButton.IsChecked = true;
                }
            }
        }

        public event EventHandler<PageRange> PageRangeChanged;
        public event EventHandler<string> CustomPageRangeChanged;
        public event EventHandler<bool> SeparateChanged;
        public event EventHandler<bool> DeleteChanged;
        public event EventHandler ExtractEvent;
        public event EventHandler CancelEvent;

        public CPDFPageExtractUI()
        {
            InitializeComponent();
        }

        private void PageRangeRadioButtonClick(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            PageRange pageRange = (PageRange)System.Enum.Parse(typeof(PageRange), radioButton.Tag.ToString());
            if(pageRange == PageRange.AllPages)
            {
                DeleteCheckBox.IsChecked = false;
                DeleteChanged?.Invoke(null, (bool)false);
            }
            PageRangeChanged?.Invoke(sender, pageRange);
        }

        private void SeparateCheckBoxClick(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            SeparateChanged?.Invoke(sender, (bool)checkBox.IsChecked);
        }

        private void DeleteCheckBoxClick(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            DeleteChanged?.Invoke(sender, (bool)checkBox.IsChecked);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            CustomPageRangeChanged?.Invoke(sender, textBox.Text);
        }

        private void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            ExtractEvent?.Invoke(null, EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelEvent?.Invoke(null, EventArgs.Empty);
        }
    }

    public enum PageRange
    {
        AllPages = 1,
        OddPages,
        EvenPages,
        CustomPages
    }
}
