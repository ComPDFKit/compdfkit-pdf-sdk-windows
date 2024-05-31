using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CheckBoxProperty : UserControl
    {
        private CheckBoxParam widgetParam = null;
        private CPDFCheckBoxWidget cPDFAnnotation = null;
        private PDFViewControl pdfViewerControl = null;
        private CPDFDocument cPDFDocument = null;
        bool IsLoadedData = false;
        public CheckBoxProperty()
        {
            InitializeComponent();
        }


        #region Loaded

        public void SetProperty(AnnotParam annotParam, CPDFAnnotation annotation, CPDFDocument doc, PDFViewControl cPDFViewer)
        {
            widgetParam = (CheckBoxParam)annotParam;
            cPDFAnnotation = (CPDFCheckBoxWidget)annotation;
            pdfViewerControl = cPDFViewer;
            cPDFDocument = doc;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FieldNameText.Text = widgetParam.FieldName;
            FormFieldCmb.SelectedIndex = (int)ParamConverter.ConverterWidgetFormFlags(widgetParam.Flags, widgetParam.IsHidden);
            BorderColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.LineColor));
            BackgroundColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.BgColor));
            CheckButtonStyleCmb.SelectedIndex = (int)widgetParam.CheckStyle;
            chkSelected.IsChecked = widgetParam.IsChecked;
            IsLoadedData = true;

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
                string text = (sender as TextBox).Text;
                if(cPDFAnnotation.GetFieldName() != text)
                {
                    CheckBoxHistory history = new CheckBoxHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    
                    cPDFAnnotation.SetFieldName(text);
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
                pdfViewerControl.UpdateAnnotFrame();
            }
        }

        private void FormFieldCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                CheckBoxHistory history = new CheckBoxHistory();
                history.Action = HistoryAction.Update;
                history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                cPDFAnnotation.SetFlags( ParamConverter.GetFormFlags((ParamConverter.FormField)(sender as ComboBox).SelectedIndex, cPDFAnnotation));
                pdfViewerControl.UpdateAnnotFrame();
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
        }

        private void BorderColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                byte[] Color = new byte[3];
                Color[0] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.R;
                Color[1] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.G;
                Color[2] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.B;

                byte[] oldColor = new byte[3];
                cPDFAnnotation.GetWidgetBorderRGBColor(ref oldColor);
                if (!oldColor.SequenceEqual(Color))
                {
                    CheckBoxHistory history = new CheckBoxHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    
                    cPDFAnnotation.SetWidgetBorderRGBColor(Color);
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
                
            }
        }

        private void BackgroundColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                byte[] Color = new byte[3];
                Color[0] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.R;
                Color[1] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.G;
                Color[2] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.B;
                
                byte[] oldColor = new byte[3];
                cPDFAnnotation.GetWidgetBgRGBColor(ref oldColor);
                if (!oldColor.SequenceEqual(Color))
                {
                    CheckBoxHistory history = new CheckBoxHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    
                    cPDFAnnotation.SetWidgetBgRGBColor(Color);
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
        }

        private void CheckButtonStyleCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                C_CHECK_STYLE style = (C_CHECK_STYLE)(sender as ComboBox).SelectedIndex;
                if (cPDFAnnotation.GetWidgetCheckStyle() != style)
                {
                    CheckBoxHistory history = new CheckBoxHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    
                    cPDFAnnotation.SetWidgetCheckStyle(style);
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
        }

        private void chkSelected_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoadedData)
            {
                if (!cPDFAnnotation.IsChecked())
                {
                    CheckBoxHistory history = new CheckBoxHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);

                    cPDFAnnotation.SetChecked(true);
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
        }

        private void chkSelected_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLoadedData)
            {
                if (cPDFAnnotation.IsChecked())
                {
                    CheckBoxHistory history = new CheckBoxHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    
                    cPDFAnnotation.SetChecked(false);
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
        }
    }
}
