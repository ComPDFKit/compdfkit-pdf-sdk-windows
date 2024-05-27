using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFAnnotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class RadioButtonProperty : UserControl
    {
        private RadioButtonParam widgetParam = null;
        private CPDFRadioButtonWidget cPDFAnnotation = null;
        private PDFViewControl pdfViewerControl = null;
        private CPDFDocument cPDFDocument = null;

        bool IsLoadedData = false;
        public RadioButtonProperty()
        {
            InitializeComponent();
        }

        #region Loaded
        public void SetProperty(AnnotParam annotParam, CPDFAnnotation annotation, CPDFDocument doc, PDFViewControl cPDFViewer)
        {
            widgetParam = (RadioButtonParam)annotParam;
            cPDFAnnotation = (CPDFRadioButtonWidget)annotation;
            pdfViewerControl = cPDFViewer;
            cPDFDocument = doc;
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FieldNameText.Text = widgetParam.FieldName;
            FormFieldCmb.SelectedIndex = (int)ParamConverter.ConverterWidgetFormFlags(widgetParam.Flags, widgetParam.IsHidden);
            BorderColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.LineColor));
            BackgroundColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.BgColor));
            RadioButtonStyleCmb.SelectedIndex = (int)widgetParam.CheckStyle;
            chkSelected.IsChecked = widgetParam.IsChecked;

            if (IsShowWarning())
            {
                WarningPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                WarningPanel.Visibility = Visibility.Visible;
            }

            IsLoadedData = true;
        }

        private bool IsShowWarning()
        {
            int count = 0;
            var page = cPDFDocument.PageAtIndex(pdfViewerControl.PDFViewTool.GetCPDFViewer().CurrentRenderFrame.PageIndex);
            List<CPDFAnnotation> annotList = page.GetAnnotations();
            if (annotList != null && annotList.Count > 0)
            {
                count = annotList.Where(x => x.Type == C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
                    .Cast<CPDFWidget>().Where(x => x.WidgetType == C_WIDGET_TYPE.WIDGET_RADIOBUTTON).Count();
            }

            return count>1;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoadedData = false;
        }

        #endregion

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

        private void RadioButtonStyleCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                cPDFAnnotation.SetWidgetCheckStyle((C_CHECK_STYLE)(sender as ComboBox).SelectedIndex);
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }

        private void chkSelected_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                cPDFAnnotation.SetChecked(true);
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }

        private void chkSelected_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLoadedData)
            {
                var history = GetNewHistory();
                cPDFAnnotation.SetChecked(false);
                pdfViewerControl.UpdateAnnotFrame();
                AddHistory(history);
            }
        }

        private RadioButtonHistory GetNewHistory()
        {
            RadioButtonHistory history = new RadioButtonHistory();
            history.Action = HistoryAction.Update;
            history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);

            return history;
        }
        
        private void AddHistory(RadioButtonHistory history)
        {
            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
            pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
        }
    }
}
