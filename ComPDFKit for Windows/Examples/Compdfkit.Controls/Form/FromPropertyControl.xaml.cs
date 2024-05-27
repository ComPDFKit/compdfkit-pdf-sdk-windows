using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool;
using ComPDFKit.Controls.Annotation.PDFAnnotationPanel.PDFAnnotationUI;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class FromPropertyControl : UserControl
    {
        private PDFViewControl pdfViewerControl; 
        private UIElement currentPanel = null;
        public FromPropertyControl()
        {
            InitializeComponent();
        }

        public void CleanProperty()
        {
            SetAnnotationPanel(null);
        }

        public void SetPropertyForType(AnnotParam annotParam, CPDFAnnotation annotation, CPDFDocument cPDFDocument) 
        {
            currentPanel = null;
            if (annotParam == null|| !(annotParam is WidgetParm))
            {
                SetAnnotationPanel(currentPanel);
                return;
            }
            switch ((annotParam as WidgetParm).WidgetType)
            {
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                    PushButtonProperty pushButtonProperty = new PushButtonProperty();
                    pushButtonProperty.SetProperty(annotParam, annotation, cPDFDocument, pdfViewerControl);
                    currentPanel = pushButtonProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_CHECKBOX:
                    CheckBoxProperty checkBoxProperty = new CheckBoxProperty();
                    checkBoxProperty.SetProperty(annotParam, annotation, cPDFDocument, pdfViewerControl);
                    currentPanel = checkBoxProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                    RadioButtonProperty radioBoxProperty = new RadioButtonProperty();
                    radioBoxProperty.SetProperty(annotParam, annotation, cPDFDocument, pdfViewerControl);
                    currentPanel = radioBoxProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                    TextFieldProperty textFieldProperty = new TextFieldProperty();
                    textFieldProperty.SetProperty(annotParam, annotation, cPDFDocument, pdfViewerControl);
                    currentPanel = textFieldProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_COMBOBOX:
                    ComboBoxProperty comboBoxProperty = new ComboBoxProperty();
                    comboBoxProperty.SetProperty(annotParam, annotation, cPDFDocument, pdfViewerControl);
                    currentPanel = comboBoxProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_LISTBOX:
                    ListBoxProperty listBoxProperty = new ListBoxProperty();
                    listBoxProperty.SetProperty(annotParam, annotation, cPDFDocument, pdfViewerControl);
                    currentPanel = listBoxProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS:
                        SignatureProperty signatureProperty = new SignatureProperty();
                        signatureProperty.SetProperty(annotParam, annotation, cPDFDocument, pdfViewerControl);
                        currentPanel = signatureProperty; 
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_UNKNOWN:
                    break;
                default:
                    break;
            }
            SetAnnotationPanel(currentPanel);
        }
        private void SetAnnotationPanel(UIElement newChild)
        {
            FromPropertyPanel.Child = newChild;
        }

        public void SetPDFViewer(PDFViewControl pdfViewer)
        {
            this.pdfViewerControl = pdfViewer;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
