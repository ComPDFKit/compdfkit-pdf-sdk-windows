using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using System.Windows;
using System.Windows.Controls;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class SignatureProperty : UserControl
    {
        private SignatureParam widgetParam = null;
        private CPDFSignatureWidget cPDFAnnotation = null;
        private PDFViewControl pdfViewerControl = null;
        private CPDFDocument cPDFDocument = null;

        bool IsLoadedData = false;

        public SignatureProperty()
        {
            InitializeComponent();
        }

        public void SetProperty(AnnotParam annotParam, CPDFAnnotation annotation, CPDFDocument doc, PDFViewControl cPDFViewer)
        {
            widgetParam = (SignatureParam)annotParam;
            cPDFAnnotation = (CPDFSignatureWidget)annotation;
            pdfViewerControl = cPDFViewer;
            cPDFDocument = doc;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FormFieldCmb.SelectedIndex = (int)ParamConverter.ConverterWidgetFormFlags(widgetParam.Flags, widgetParam.IsHidden);
            IsLoadedData = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoadedData = false;
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
        
        private SignatureHistory GetNewHistory()
        {
            SignatureHistory history = new SignatureHistory();
            history.Action = HistoryAction.Update;
            history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);

            return history;
        }
        
        private void AddHistory(SignatureHistory history)
        {
            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
            pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
        }
    }
}
