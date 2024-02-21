using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Compdfkit_Tools.PDFControl
{
    public partial class TextFieldProperty : UserControl
    {

        private WidgetTextBoxArgs widgetArgs = null;
        private AnnotAttribEvent annotAttribEvent = null;
        public ObservableCollection<int> SizeList { get; set; } = new ObservableCollection<int>
        {
            6,8,9,10,12,14,18,20,24,26,28,32,30,32,48,72
        };

        bool IsLoadedData = false;

        public TextFieldProperty()
        {
            InitializeComponent();
        }

        #region Loaded

        public void SetProperty(WidgetArgs Args, AnnotAttribEvent e)
        {
            widgetArgs = (WidgetTextBoxArgs)Args;
            annotAttribEvent = e;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Binding SizeListbinding = new Binding();
            SizeListbinding.Source = this;
            SizeListbinding.Path = new System.Windows.PropertyPath("SizeList");
            FontSizeCmb.SetBinding(ComboBox.ItemsSourceProperty, SizeListbinding);

            FieldNameText.Text = widgetArgs.FieldName;
            FormFieldCmb.SelectedIndex = (int)widgetArgs.FormField;
            BorderColorPickerControl.SetCheckedForColor(widgetArgs.LineColor);
            BackgroundColorPickerControl.SetCheckedForColor(widgetArgs.BgColor);
            TextColorPickerControl.SetCheckedForColor(widgetArgs.FontColor);
            SetFontName(widgetArgs.FontName);
            SetFontStyle(widgetArgs.IsItalic, widgetArgs.IsBold);
            SetFontSize(widgetArgs.FontSize);
            TextAlignmentCmb.SelectedIndex = (int)widgetArgs.Alignment;
            DefaultText.Text = widgetArgs.Text;
            chkMutiline.IsChecked = widgetArgs.IsMultiLine;
            IsLoadedData = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoadedData = false;
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
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FontSize, (sender as ComboBox).SelectedItem );
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void TextAlignmentCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.TextAlign, (sender as ComboBox).SelectedIndex);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void DefaultText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Text, (sender as TextBox).Text);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void chkMutiline_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.IsMutilLine, true);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void chkMutiline_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.IsMutilLine, false);
                annotAttribEvent.UpdateAnnot();
            }
        }

        #endregion
    }
}
