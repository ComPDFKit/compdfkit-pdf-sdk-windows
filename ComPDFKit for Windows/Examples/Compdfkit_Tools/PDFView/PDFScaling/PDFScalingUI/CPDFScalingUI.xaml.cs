using ComPDFKitViewer.PdfViewer;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFScalingUI : UserControl, INotifyPropertyChanged
    {
        public CPDFViewer PDFView { get; set; }

        private string regixString = "[^0-9]+";


        private int _scale = 100;
        public int Scale
        {
            get { return _scale; }
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnPropertyChanged(nameof(Scale));
                }
            }
        }

        private int[] zoomLevelList = { 10, 25, 50, 100, 150, 200, 300, 400, 500, 1000 };

        public event EventHandler<string> SetScaleEvent;
        public event EventHandler<string> SetPresetScaleEvent;
        public event EventHandler ScaleIncreaseEvent;
        public event EventHandler ScaleDecreaseEvent;

        public CPDFScalingUI()
        {
            InitializeComponent();
            DataContext = this;
            BindZoomLevel();
        }

        private void DropDownNumberBoxControl_SetPresetEvent(object sender, string e)
        {
            SetPresetScaleEvent?.Invoke(this, e);
        }

        private void BindZoomLevel()
        {
            ZoomComboBox.Items.Add(new ComboBoxItem() { Content = LanguageHelper.CommonManager.GetString("Zoom_Real") });
            ZoomComboBox.Items.Add(new ComboBoxItem() { Content = LanguageHelper.CommonManager.GetString("Zoom_FitWidth") });
            ZoomComboBox.Items.Add(new ComboBoxItem() { Content = LanguageHelper.CommonManager.GetString("Zoom_FitPage")});
            foreach (double zoomLevel in zoomLevelList)
            {
                ComboBoxItem zoomItem = new ComboBoxItem();
                zoomItem.Content = zoomLevel;
                ZoomComboBox.Items.Add(zoomItem);
            }
        }

        private void ZoomComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectItem = ZoomComboBox.SelectedItem as ComboBoxItem;
            SetPresetScaleEvent?.Invoke(this, selectItem.Content.ToString());
        }

        public void SetZoomTextBoxText(string value)
        {
            ZoomTextBox.Text = value;
        }

        private void DropDownNumberBoxControl_InputEnterEvent(object sender, string e)
        {
            SetScaleEvent?.Invoke(this,e);
        }

        private void ScaleDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            ScaleDecreaseEvent?.Invoke(this,e);
        }

        private void ScaleIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            ScaleIncreaseEvent?.Invoke(this,e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex(regixString).IsMatch(e.Text);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetScaleEvent?.Invoke(sender, ZoomTextBox.Text);
            }
            if ((e.KeyStates == Keyboard.GetKeyStates(Key.LeftCtrl) || e.KeyStates == Keyboard.GetKeyStates(Key.RightCtrl)) && e.KeyStates == Keyboard.GetKeyStates(Key.V))
                e.Handled = true;
            else
                e.Handled = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ZoomTextBox.Text))
            {
                int num;
                int.TryParse(ZoomTextBox.Text, out num);
                if (num > 1000)
                {
                    ZoomTextBox.Text = 1000.ToString();
                }

                if (num < 1)
                {
                    ZoomTextBox.Text = 1.ToString();
                }
            }
        }

        private void ZoomTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SetScaleEvent?.Invoke(sender, ZoomTextBox.Text);
        }
    }
}
