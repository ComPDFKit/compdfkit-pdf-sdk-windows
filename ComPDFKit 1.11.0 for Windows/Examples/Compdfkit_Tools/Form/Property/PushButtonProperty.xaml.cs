using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Compdfkit_Tools.Helper;
using ComPDFKit.PDFDocument.Action;
using ComPDFKitViewer.PdfViewer;

namespace Compdfkit_Tools.PDFControl
{
    public partial class PushButtonProperty : UserControl,INotifyPropertyChanged
    {
        private WidgetPushButtonArgs widgetArgs = null;
        private AnnotAttribEvent annotAttribEvent = null;
        private CPDFViewer pdfViewer = null;
        public ObservableCollection<int> SizeList { get; set; } = new ObservableCollection<int>
        {
            6,8,9,10,12,14,18,20,24,26,28,32,30,32,48,72
        };

        private string _HintText;
        public string HintText
        {
            get => _HintText;
            set => SetField(ref _HintText, value);
        }

        bool IsLoadedData = false;
        public PushButtonProperty()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region Loaded

        public void SetProperty(WidgetArgs Args, AnnotAttribEvent e, CPDFViewer cPDFViewer)
        {
            pdfViewer = cPDFViewer;
            widgetArgs = (WidgetPushButtonArgs)Args;
            annotAttribEvent = e;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Binding SizeListbinding = new Binding();
            SizeListbinding.Source = this;
            SizeListbinding.Path = new System.Windows.PropertyPath("SizeList");
            FontSizeCmb.SetBinding(ComboBox.ItemsSourceProperty, SizeListbinding);

            TopTabControl.SelectedIndex = 2;
            FieldNameText.Text = widgetArgs.FieldName;
            FormFieldCmb.SelectedIndex = (int)widgetArgs.FormField;
            BorderColorPickerControl.SetCheckedForColor(widgetArgs.LineColor);
            BackgroundColorPickerControl.SetCheckedForColor(widgetArgs.BgColor);
            TextColorPickerControl.SetCheckedForColor(widgetArgs.FontColor);
            SetFontName(widgetArgs.FontName);
            SetFontStyle(widgetArgs.IsItalic, widgetArgs.IsBold);
            SetFontSize(widgetArgs.FontSize);
            ItemText.Text = widgetArgs.Text;
            SetActionContext(widgetArgs.ActionDict);
            IsLoadedData = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoadedData = false;
        }

        private void SetActionContext(Dictionary<C_ACTION_TYPE, string> keyValuePairs)
        {
            foreach (C_ACTION_TYPE key in keyValuePairs.Keys)
            {
                if (key == C_ACTION_TYPE.ACTION_TYPE_GOTO)
                {
                    TextAlignmentCmb.SelectedIndex = 1; 
                    ActionContentText.Visibility = Visibility.Visible;
                    ActionContentText.Text = (Convert.ToInt32( keyValuePairs[key])+1).ToString();
                    break;
                }
                if (key == C_ACTION_TYPE.ACTION_TYPE_URI)
                {
                    TextAlignmentCmb.SelectedIndex = 2; 
                    ActionContentText.Visibility = Visibility.Visible;
                    ActionContentText.Text = keyValuePairs[key];
                    break;
                }
                else
                {
                    TextAlignmentCmb.SelectedIndex = 0;
                    ActionContentText.Text = "";
                    break;
                }
            }
        }
        private void SetFontSize(double size)
        {
            int index = SizeList.IndexOf((int)size);
            FontSizeCmb.SelectedIndex = index;
        }

        private void SetFontStyle(bool IsItalic, bool IsBold)
        {
            int index = 0;
            if (IsItalic && IsBold)
            {
                index = 3;
            }
            else if (IsItalic)
            {
                index = 2;
            }
            else if (IsBold)
            {
                index = 1;
            }
            FontStyleCmb.SelectedIndex = index;
        }

        private void SetFontName(string fontName)
        {
            int index = -1;
            List<string> fontFamilyList = new List<string>() { "Helvetica", "Courier", "Times" };
            for (int i = 0; i < fontFamilyList.Count; i++)
            {
                if (fontFamilyList[i].ToLower().Contains(fontName.ToLower())
                    || fontName.ToLower().Contains(fontFamilyList[i].ToLower()))
                {
                    index = i;
                }
            }
            FontCmb.SelectedIndex = index;
        }

        #endregion

        #region Updata

        private void FieldNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FieldName, (sender as TextBox).Text);
                annotAttribEvent.UpdateAnnot();
            }
        }


        private void FormFieldCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FormField, (sender as ComboBox).SelectedIndex);
                annotAttribEvent.UpdateAnnot();
            }
        }


        private void BorderColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Color, ((SolidColorBrush)BorderColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
        }


        private void BackgroundColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FillColor, ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void TextColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FontColor, ((SolidColorBrush)TextColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void FontCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FontName, ((sender as ComboBox).SelectedItem as ComboBoxItem).Content.ToString());
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void FontStyleCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                bool IsItalic = false;
                bool IsBold = false;
                switch ((sender as ComboBox).SelectedIndex)
                {
                    case 0:
                        break;
                    case 1:
                        IsBold = true;
                        break;
                    case 2:
                        IsItalic = true;
                        break;
                    case 3:
                        IsItalic = true;
                        IsBold = true;
                        break;
                    default:
                        break;
                }
                annotAttribEvent.UpdateAttrib(AnnotAttrib.IsBold, IsBold);
                annotAttribEvent.UpdateAttrib(AnnotAttrib.IsItalic, IsItalic);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void FontSizeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FontSize, (sender as ComboBox).SelectedItem);
                annotAttribEvent.UpdateAnnot();
            }
        }
        #endregion

        private void TextAlignmentCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                ActionContentText.Style = null;
                switch (TextAlignmentCmb.SelectedIndex)
                {
                    case 0:
                        ActionContentText.Visibility = Visibility.Collapsed;
                        break;
                    case 1:
                        ActionContentText.Visibility = Visibility.Visible;
                        HintText = LanguageHelper.PropertyPanelManager.GetString("Holder_Jump") + pdfViewer.Document.PageCount;
                        ActionContentText.Style = this.FindResource("txtboxStyle") as Style;
                        ActionContentText.Text = "";
                        break;
                    case 2:
                        ActionContentText.Visibility = Visibility.Visible;
                        HintText = "https://www.compdf.com";
                        ActionContentText.Style = this.FindResource("txtboxStyle") as Style;
                        ActionContentText.Text = "";
                        break;
                    default:
                        break;
                }
                if (ActionContentText.Visibility != Visibility.Collapsed && ActionContentText != null)
                    AddAction();
            }
        }
        private void AddAction()
        {
            Dictionary<C_ACTION_TYPE, string> ActionDict = new Dictionary<C_ACTION_TYPE, string>();
            if (TextAlignmentCmb.SelectedIndex == 1 && !string.IsNullOrEmpty(TextAlignmentCmb.Text))
            {
                int page = 0;
                int.TryParse(ActionContentText.Text.Trim(), out page);
                if (page <= 0 || page > pdfViewer.Document.PageCount)
                    page = 1;
                if (page - 1 >= 0)
                    ActionDict[C_ACTION_TYPE.ACTION_TYPE_GOTO] = (page - 1).ToString();
            }
            if (TextAlignmentCmb.SelectedIndex == 2)
            {
                if (string.IsNullOrEmpty(ActionContentText.Text.Trim()))
                {
                    ActionDict[C_ACTION_TYPE.ACTION_TYPE_URI] = @"https://www.compdf.com";
                }
                else
                {
                    ActionDict[C_ACTION_TYPE.ACTION_TYPE_URI] = ActionContentText.Text.Trim();
                }
            }

            if (ActionDict != null && ActionDict.Count > 0)
            {
                annotAttribEvent?.UpdateAttrib(AnnotAttrib.FormAction, ActionDict);
                annotAttribEvent?.UpdateAnnot();
            }

        }
        private void ItemText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Text, (sender as TextBox).Text);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void ActionContentText_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddAction();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
