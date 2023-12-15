using Compdfkit_Tools.Common;
using Compdfkit_Tools.Helper;
using ComPDFKitViewer.PdfViewer;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFPageInsertUI : UserControl, INotifyPropertyChanged
    {
        private string password = string.Empty;

        private int _maxIndex;
        public int MaxIndex
        {
            get => _maxIndex;
            set
            {
                _maxIndex = value;
                PageTextBox.Maxium = _maxIndex;
                MaxPageTextBox.Text = _maxIndex.ToString();
            }
        }

        private int insertPageCount = 0;

        private int _customPageIndex;
        public int CustomPageIndex
        {
            get => _customPageIndex;
            set
            {
                _customPageIndex = value;
                OnPropertyChanged();
                CustomPageLocationChange(value);
            }
        }

        public PageInsertLocation PageInsertLocation
        {
            set
            {
                if (value == PageInsertLocation.HomePage)
                {
                    HomePageRadioButton.IsChecked = true;
                }
                else if (value == PageInsertLocation.GastricPage)
                {
                    GastricPageRadioButton.IsChecked = true;
                }
                else if (value == PageInsertLocation.CustomPage)
                {
                    CustomPageRadioButton.IsChecked = true;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<string> InsertTypeChanged;
        public event EventHandler<string> SelectedFileChanged;
        public event EventHandler<string> PasswordChanged;
        public event EventHandler<string> TextChanged;
        public event EventHandler<int> InsertIndexChanged;
        public event EventHandler<double[]> PageSizeChanged;
        public event EventHandler CancelEvent;
        public event EventHandler InsertEvent;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CPDFPageInsertUI()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelEvent?.Invoke(null, EventArgs.Empty);
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            InsertEvent?.Invoke(null, EventArgs.Empty);
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = CommonHelper.GetExistedPathOrEmpty();

            if (filePath != string.Empty)
            {
                CPDFViewer pdfViewer = new CPDFViewer();
                pdfViewer.InitDocument(filePath);
                if (pdfViewer.Document.IsLocked)
                {
                    PasswordWindow passwordWindow = new PasswordWindow();
                    Window parentWindow = Window.GetWindow(this);
                    passwordWindow.Owner = parentWindow;
                    passwordWindow.InitWithPDFViewer(pdfViewer);
                    passwordWindow.DialogClosed += PasswordWindow_DialogClosed;
                    passwordWindow.ShowDialog();
                    if (password != string.Empty)
                    {
                        WritableComboBoxControl.MaxPageIndex = pdfViewer.Document.PageCount;
                        FilePathTextBox.Text = filePath;
                        SelectedFileChanged?.Invoke(sender, filePath);
                    }
                }
                else
                {
                    WritableComboBoxControl.MaxPageIndex = pdfViewer.Document.PageCount;
                    FilePathTextBox.Text = filePath;
                    PasswordChanged?.Invoke(sender, string.Empty); 
                    SelectedFileChanged?.Invoke(sender, filePath);
                } 
            }
        }

        private void PasswordWindow_DialogClosed(object sender, PasswordEventArgs e)
        {
            if(e.DialogResult != string.Empty)
            {
                password = e.DialogResult;
                PasswordChanged?.Invoke(sender, e.DialogResult);
            }
        }

        private void PageInsertLocation_Click(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton.Tag.ToString() == "HomePage")
            {
                InsertIndexChanged?.Invoke(sender, 0);
            }
            else if (radioButton.Tag.ToString() == "GastricPage")
            {
                InsertIndexChanged?.Invoke(sender, MaxIndex);
            }
            else
            {
                CustomPageLocationChange();
            }
        }

        public void CustomPageLocationChange(int index = -2)
        {
            if (index == -2 && PageTextBox.Text != null)
            {
                index = int.Parse(PageTextBox.Text);
            }
            if (PageTextBox.Text != string.Empty)
            {
                if (PageLocationComboBox.SelectedIndex == 0)
                {
                    InsertIndexChanged?.Invoke(null, index);
                }
                else
                {
                    InsertIndexChanged?.Invoke(null, index - 1);
                }
            }
            else
            {
                InsertIndexChanged?.Invoke(null, -1);
            }
        }

        private void InsertTypeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            InsertTypeChanged?.Invoke(sender, radioButton.Tag.ToString());
        }

        private void PageLocationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomPageLocationChange();
        }

        private void WritableComboBoxControl_Loaded(object sender, RoutedEventArgs e)
        {
            WritableComboBoxControl.TextChanged += WritableComboBoxControl_TextChanged;
        }

        private void WritableComboBoxControl_Unloaded(object sender, RoutedEventArgs e)
        {
            WritableComboBoxControl.TextChanged -= WritableComboBoxControl_TextChanged;
        }

        private void WritableComboBoxControl_TextChanged(object sender, string e)
        {
            TextChanged?.Invoke(sender, e);
        }

        private void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)PageSizeComboBox.SelectedItem;
            string selectedContent = selectedItem.Content.ToString();
            double[] doubles = new double[2];
            if (selectedContent == "A3")
            {
                doubles[0] = 297 * 2.54;
                doubles[1] = 420 * 2.54;

            }
            else if (selectedContent == "A4")
            {
                doubles[0] = 210 * 2.54;
                doubles[1] = 297 * 2.54;
            }
            else if (selectedContent == "A5")
            {
                doubles[0] = 148 * 2.54;
                doubles[1] = 210 * 2.54;
            }
            PageSizeChanged?.Invoke(sender, doubles);
        }
    }

    public enum PageInsertLocation
    {
        HomePage,
        GastricPage,
        CustomPage
    }

}
