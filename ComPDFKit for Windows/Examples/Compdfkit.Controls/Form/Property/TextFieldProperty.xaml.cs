using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;
using System.Linq;
using System.Windows.Input;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class TextFieldProperty : UserControl
    { 
        public bool isFontStyleCmbClicked;

        private TextBoxParam widgetParam = null;
        private CPDFTextWidget cPDFAnnotation = null;
        private PDFViewControl pdfViewerControl = null;
        private CPDFDocument cPDFDocument = null;
        public ObservableCollection<int> SizeList { get; set; } = new ObservableCollection<int>
        {
            6,8,9,10,12,14,18,20,24,26,28,30,32,48,72
        };

        bool IsLoadedData = false;

        public TextFieldProperty()
        {
            InitializeComponent();
        }

        #region Loaded

        public void SetProperty(AnnotParam annotParam, CPDFAnnotation annotation, CPDFDocument doc, PDFViewControl cPDFViewer)
        {
            widgetParam = (TextBoxParam)annotParam;
            cPDFAnnotation = (CPDFTextWidget)annotation;
            pdfViewerControl = cPDFViewer;
            cPDFDocument = doc;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Binding SizeListbinding = new Binding();
            SizeListbinding.Source = this;
            SizeListbinding.Path = new System.Windows.PropertyPath("SizeList");
            FontSizeCmb.SetBinding(ComboBox.ItemsSourceProperty, SizeListbinding);

            FieldNameText.Text = widgetParam.FieldName;
            FormFieldCmb.SelectedIndex = (int)ParamConverter.ConverterWidgetFormFlags(widgetParam.Flags, widgetParam.IsHidden);
            BorderColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.LineColor));
            BackgroundColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.BgColor));
            TextColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.FontColor));
            string familyName = string.Empty;
            string styleName = string.Empty;

            CPDFFont.GetFamilyStyleName(widgetParam.FontName, ref familyName, ref styleName);

            FontCmb.ItemsSource = CPDFFont.GetFontNameDictionary().Keys.ToList();

            SetFontName(familyName);
            SetFontStyle(styleName);
            SetFontSize(widgetParam.FontSize);
             
            TextAlignmentCmb.SelectedIndex = (int)widgetParam.Alignment;
            DefaultText.Text = widgetParam.Text;
            chkMutiline.IsChecked = widgetParam.IsMultiLine;
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

        private void SetFontStyle(string fontStyle)
        {
            FontStyleCmb.SelectedValue = fontStyle;
        }

        private void SetFontName(string fontName)
        {
            FontCmb.SelectedValue = fontName;
            if (FontCmb.SelectedValue != null)
            {
                FontStyleCmb.ItemsSource = CPDFFont.GetFontNameDictionary()[FontCmb.SelectedValue.ToString()];
            }
        }

        #endregion

        #region Updata

        private void FieldNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                cPDFAnnotation.SetFieldName((sender as TextBox).Text);
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }


        private void FormFieldCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                cPDFAnnotation.SetFlags(ParamConverter.GetFormFlags((ParamConverter.FormField)(sender as ComboBox).SelectedIndex, cPDFAnnotation));
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }


        private void BorderColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                byte[] Color = new byte[3];
                Color[0] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.R;
                Color[1] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.G;
                Color[2] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.B;
                cPDFAnnotation.SetWidgetBorderRGBColor(Color);
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }


        private void BackgroundColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                byte[] Color = new byte[3];
                Color[0] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.R;
                Color[1] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.G;
                Color[2] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.B;
                cPDFAnnotation.SetWidgetBgRGBColor(Color);
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }

        private void TextColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                byte[] Color = new byte[3];
                Color[0] = ((SolidColorBrush)TextColorPickerControl.Brush).Color.R;
                Color[1] = ((SolidColorBrush)TextColorPickerControl.Brush).Color.G;
                Color[2] = ((SolidColorBrush)TextColorPickerControl.Brush).Color.B;
                CTextAttribute cTextAttribute = cPDFAnnotation.GetTextAttribute();
                cTextAttribute.FontColor = Color;
                cPDFAnnotation.SetTextAttribute(cTextAttribute);
                cPDFAnnotation.UpdateFormAp();
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }

        private void FontCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                FontStyleCmb.ItemsSource = CPDFFont.GetFontNameDictionary()[FontCmb.SelectedValue.ToString()];
                FontStyleCmb.SelectedIndex = 0;

                var history = GetNewHistory();
                CTextAttribute cTextAttribute = cPDFAnnotation.GetTextAttribute();

                string psFontName = string.Empty;
                CPDFFont.GetPostScriptName(FontCmb.SelectedValue.ToString(), FontStyleCmb.SelectedValue.ToString(), ref psFontName);
                cTextAttribute.FontName = psFontName;

                cPDFAnnotation.SetTextAttribute(cTextAttribute);
                cPDFAnnotation.UpdateFormAp();
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }

        private void FontStyleCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            { 
                    var history = GetNewHistory();
                    CTextAttribute cTextAttribute = cPDFAnnotation.GetTextAttribute();
                    string psFontName = string.Empty;
                    CPDFFont.GetPostScriptName(FontCmb.SelectedValue.ToString(), FontStyleCmb.SelectedValue?.ToString(), ref psFontName);
                    cTextAttribute.FontName = psFontName;
                    cPDFAnnotation.SetTextAttribute(cTextAttribute);
                    cPDFAnnotation.UpdateFormAp();
                    pdfViewerControl.UpdateAnnotFrame();
                    AddHistory(history);
                }
                else
                {
                    FontStyleCmb.ItemsSource = CPDFFont.GetFontNameDictionary()[FontCmb.SelectedValue.ToString()];
                }  
        }

        private void FontSizeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            { 
                    var history = GetNewHistory();
                    CTextAttribute cTextAttribute = cPDFAnnotation.GetTextAttribute();
                    cTextAttribute.FontSize = Convert.ToSingle((sender as ComboBox).SelectedItem);
                    cPDFAnnotation.SetTextAttribute(cTextAttribute);
                    cPDFAnnotation.UpdateFormAp();
                    pdfViewerControl.UpdateAnnotFrame();
                    AddHistory(history);
                }  
        }

        private void TextAlignmentCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                cPDFAnnotation.SetJustification((C_TEXT_ALIGNMENT)(sender as ComboBox).SelectedIndex);
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }

        private void DefaultText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                cPDFAnnotation.SetText((sender as TextBox).Text);
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }

        private void chkMutiline_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                cPDFAnnotation.SetMultiLine(true);
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }

        private void chkMutiline_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                cPDFAnnotation.SetMultiLine(false);
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }
        
        private TextBoxHistory GetNewHistory()
        {
            TextBoxHistory history = new TextBoxHistory();
            history.Action = HistoryAction.Update;
            history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);

            return history;
        }
        
        private void AddHistory(TextBoxHistory history)
        {
            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
            pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
        }

        #endregion
    }
}
