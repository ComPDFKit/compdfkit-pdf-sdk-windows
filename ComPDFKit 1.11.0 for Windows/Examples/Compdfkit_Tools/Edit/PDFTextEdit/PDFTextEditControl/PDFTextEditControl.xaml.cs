using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using ComPDFKitViewer;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Compdfkit_Tools.Edit
{
    public partial class PDFTextEditControl : UserControl
    {
        public CPDFViewer PDFView { get; private set; }
        public PDFEditEvent EditEvent { get; set; }

        public List<PDFEditEvent> EditMultiEvents { get; set; }

        public PDFTextEditControl()
        {
            InitializeComponent();
            Loaded += PDFTextEditControl_Loaded;
        }

        public void InitWithPDFViewer(CPDFViewer newPDFView)
        {
            PDFView = newPDFView;
        }

        public void SetPDFTextEditData(PDFEditEvent newEvent)
        {
            EditEvent = null;
            EditMultiEvents = null;
            if (newEvent != null && newEvent.EditType == CPDFEditType.EditText)
            {
                if (newEvent.SystemFontNameList != null && newEvent.SystemFontNameList.Count == 0)
                {
                    newEvent.SystemFontNameList.Add("Helvetica");
                    newEvent.SystemFontNameList.Add("Courier New");
                    newEvent.SystemFontNameList.Add("Times New Roman");
                }
                if (newEvent.SystemFontNameList.Contains(newEvent.FontName) == false && string.IsNullOrEmpty(newEvent.FontName) == false)
                {
                    newEvent.SystemFontNameList.Add(newEvent.FontName);
                }

                TextStyleUI.SetFontNames(newEvent.SystemFontNameList);
                TextStyleUI.SelectFontName(newEvent.FontName);
                TextStyleUI.SetFontStyle(newEvent.IsBold, newEvent.IsItalic);
                TextStyleUI.SetFontSize(newEvent.FontSize);
                OpacityTextBox.Text = string.Format("{0}%", (int)(Math.Ceiling(newEvent.Transparency * 100 / 255D)));
                FontOpacitySlider.Value = ((int)(Math.Ceiling(newEvent.Transparency * 100 / 255D))) / 100D;
                TextAlignUI.SetFontAlign(newEvent.TextAlign);
                FontColorUI.SetCheckedForColor(newEvent.FontColor);
            }
            EditEvent = newEvent;
        }

        public void SetPDFTextMultiEditData(List<PDFEditEvent> editEvents)
        {
            EditEvent = null;
            EditMultiEvents = null;
            if(editEvents!=null && editEvents.Count>0)
            {
                PDFEditEvent editEvent= editEvents[0];

                if (editEvent != null && editEvent.EditType == CPDFEditType.EditText)
                {
                    if (editEvent.SystemFontNameList != null && editEvent.SystemFontNameList.Count == 0)
                    {
                        editEvent.SystemFontNameList.Add("Helvetica");
                        editEvent.SystemFontNameList.Add("Courier New");
                        editEvent.SystemFontNameList.Add("Times New Roman");
                    }
                    if (editEvent.SystemFontNameList.Contains(editEvent.FontName) == false && string.IsNullOrEmpty(editEvent.FontName) == false)
                    {
                        editEvent.SystemFontNameList.Add(editEvent.FontName);
                    }

                    TextStyleUI.SetFontNames(editEvent.SystemFontNameList);
                    TextStyleUI.SelectFontName(editEvent.FontName);
                    TextStyleUI.SetFontStyle(editEvent.IsBold, editEvent.IsItalic);
                    TextStyleUI.SetFontSize(editEvent.FontSize);
                    OpacityTextBox.Text = string.Format("{0}%", (int)(Math.Ceiling(editEvent.Transparency * 100 / 255D)));
                    FontOpacitySlider.Value = ((int)(Math.Ceiling(editEvent.Transparency * 100 / 255D))) / 100D;
                    TextAlignUI.SetFontAlign(editEvent.TextAlign);
                    FontColorUI.SetCheckedForColor(editEvent.FontColor);
                }
            }

            EditMultiEvents=editEvents;

        }
        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                slider.Tag = "true";
            }
            if (EditEvent != null)
            {
                EditEvent.FontSize = slider.Value;
                EditEvent.UpdatePDFEditByEventArgs();
            }

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.FontSize = slider.Value;
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void SliderOpacity_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                slider.Tag = "true";
            }
            if (EditEvent != null)
            {
                EditEvent.Transparency = (int)(FontOpacitySlider.Value * 255);
                EditEvent.UpdatePDFEditByEventArgs();
            }

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.Transparency = (int)(FontOpacitySlider.Value * 255);
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (OpacityTextBox != null && FontOpacitySlider != null)
            {
                OpacityTextBox.Text = string.Format("{0}%", (int)(FontOpacitySlider.Value * 100));
            }

            if (slider != null && slider.Tag != null && slider.Tag.ToString() == "false")
            {
                return;
            }

            if (EditEvent != null)
            {
                EditEvent.Transparency = (int)(FontOpacitySlider.Value * 255);
                EditEvent.UpdatePDFEditByEventArgs();
            }

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.Transparency = (int)(FontOpacitySlider.Value * 255);
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                slider.Tag = "false";
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (slider != null && slider.Tag != null && slider.Tag.ToString() == "false")
            {
                return;
            }

            if (EditEvent != null)
            {
                EditEvent.FontSize = slider.Value;
                EditEvent.UpdatePDFEditByEventArgs();
            }

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.FontSize = slider.Value;
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void PDFTextEditControl_Loaded(object sender, RoutedEventArgs e)
        {

            TextStyleUI.TextFontChanged -= TextStyleUI_TextFontChanged;
            TextStyleUI.TextBoldChanged -= TextStyleUI_TextBoldChanged;
            TextStyleUI.TextItalicChanged -= TextStyleUI_TextItalicChanged;
            TextStyleUI.TextSizeChanged -= TextStyleUI_TextSizeChanged;
            TextAlignUI.TextAlignChanged -= TextAlignUI_TextAlignChanged;
            FontColorUI.ColorChanged -= FontColorUI_ColorChanged;

            TextStyleUI.TextFontChanged += TextStyleUI_TextFontChanged;
            TextStyleUI.TextBoldChanged += TextStyleUI_TextBoldChanged;
            TextStyleUI.TextItalicChanged += TextStyleUI_TextItalicChanged;
            TextStyleUI.TextSizeChanged += TextStyleUI_TextSizeChanged;
            TextAlignUI.TextAlignChanged += TextAlignUI_TextAlignChanged;
            FontColorUI.ColorChanged += FontColorUI_ColorChanged;
        }

        private void TextStyleUI_TextSizeChanged(object sender, double e)
        {
            if (EditEvent != null)
            {
                EditEvent.FontSize = e;
                EditEvent.UpdatePDFEditByEventArgs();
            }

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.FontSize = e;
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void FontColorUI_ColorChanged(object sender, EventArgs e)
        {
            SolidColorBrush newBrush = FontColorUI.Brush as SolidColorBrush;

            if (EditEvent != null && newBrush!=null)
            {
                EditEvent.FontColor = newBrush.Color;
                EditEvent.UpdatePDFEditByEventArgs();
            }

            if (EditMultiEvents != null && newBrush != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.FontColor = newBrush.Color;
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void TextAlignUI_TextAlignChanged(object sender, TextAlignType e)
        {
            if (EditEvent != null)
            {
                EditEvent.TextAlign = e;
                EditEvent.UpdatePDFEditByEventArgs();
            }

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.TextAlign = e;
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void TextStyleUI_TextItalicChanged(object sender, bool e)
        {
            if (EditEvent != null)
            {
                EditEvent.IsItalic = e;
                EditEvent.UpdatePDFEditByEventArgs();
            }

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.IsItalic = e;
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void TextStyleUI_TextBoldChanged(object sender, bool e)
        {
            if (EditEvent != null)
            {
                EditEvent.IsBold = e;
                EditEvent.UpdatePDFEditByEventArgs();
            }

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.IsBold = e;
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void TextStyleUI_TextFontChanged(object sender, string e)
        {
            if (EditEvent != null)
            {
                EditEvent.FontName = e;
                EditEvent.UpdatePDFEditByEventArgs();
            }

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.FontName = e;
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void OpacityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectItem = OpacityComboBox.SelectedItem as ComboBoxItem;
            if (selectItem != null && selectItem.Content != null)
            {
                if (double.TryParse(selectItem.Content.ToString().TrimEnd('%'), out double newOpacity))
                {
                    OpacityTextBox.Text = selectItem.Content.ToString();
                    FontOpacitySlider.Value = newOpacity / 100.0;
                }
            }
        }
    }
}
