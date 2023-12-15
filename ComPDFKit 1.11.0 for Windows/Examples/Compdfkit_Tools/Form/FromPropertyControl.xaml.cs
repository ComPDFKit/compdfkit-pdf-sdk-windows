using Compdfkit_Tools.Annotation.PDFAnnotationPanel.PDFAnnotationUI;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControl
{
    public partial class FromPropertyControl : UserControl
    {
        private CPDFViewer pdfViewer; 
        private UIElement currentPanel = null;
        public FromPropertyControl()
        {
            InitializeComponent();
        }

        public void CleanProperty()
        {
            SetAnnotationPanel(null);
        }

        public void SetPropertyForType(WidgetArgs Args, AnnotAttribEvent e) 
        {
            currentPanel = null;
            if (Args==null)
            {
                SetAnnotationPanel(currentPanel);
                return;
            }
            switch (Args.WidgetType)
            {
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                    PushButtonProperty pushButtonProperty = new PushButtonProperty();
                    pushButtonProperty.SetProperty(Args, e, pdfViewer);
                    currentPanel = pushButtonProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_CHECKBOX:
                    CheckBoxProperty checkBoxProperty = new CheckBoxProperty();
                    checkBoxProperty.SetProperty(Args, e);
                    currentPanel = checkBoxProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                    RadioButtonProperty radioBoxProperty = new RadioButtonProperty();
                    radioBoxProperty.SetProperty(Args, e, pdfViewer);
                    currentPanel = radioBoxProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                    TextFieldProperty textFieldProperty = new TextFieldProperty();
                    textFieldProperty.SetProperty(Args,e);
                    currentPanel = textFieldProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_COMBOBOX:
                    ComboBoxProperty comboBoxProperty = new ComboBoxProperty();
                    comboBoxProperty.SetProperty(Args, e);
                    currentPanel = comboBoxProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_LISTBOX:
                    ListBoxProperty listBoxProperty = new ListBoxProperty();
                    listBoxProperty.SetProperty(Args, e);
                    currentPanel = listBoxProperty;
                    break;
                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS:
                    if (e==null)
                    {
                        CPDFSignatureUI signatureProperty = new CPDFSignatureUI();
                        signatureProperty.SetFormProperty(Args, pdfViewer);
                        currentPanel = signatureProperty;
                    }
                    else
                    {
                        SignatureProperty signatureProperty = new SignatureProperty();
                        signatureProperty.SetProperty(Args, e);
                        currentPanel = signatureProperty;
                    }
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

        public void SetPDFViewer(CPDFViewer pdfViewer)
        {
            this.pdfViewer = pdfViewer;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
