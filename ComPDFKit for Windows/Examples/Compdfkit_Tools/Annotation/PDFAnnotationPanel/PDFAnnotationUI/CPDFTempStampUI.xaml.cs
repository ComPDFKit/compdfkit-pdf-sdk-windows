using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Compdfkit_Tools.PDFControlUI
{
    /// <summary>
    /// Interaction logic for CPDFImageUI.xaml
    /// </summary>
    public partial class CPDFTempStampUI : UserControl
    {
        private AnnotAttribEvent annotAttribEvent;

        public CPDFTempStampUI()
        {
            InitializeComponent();
        }

        public void SetPresentAnnotAttrib(AnnotAttribEvent annotAttribEvent)
        {
            this.annotAttribEvent = null;
            NoteTextBox.Text = (string)annotAttribEvent.Attribs[AnnotAttrib.NoteText];
            this.annotAttribEvent = annotAttribEvent;
            WriteableBitmap writeableBitmap = (annotAttribEvent.GetAnnotHandlerEventArgs(AnnotArgsType.AnnotStamp)[0] as StampAnnotArgs).GetStampDrawing();
            CPDFAnnotationPreviewerControl.DrawStampPreview(writeableBitmap);
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(annotAttribEvent != null)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.NoteText, NoteTextBox.Text);
                annotAttribEvent.UpdateAnnot();
            }
        }
    }
}
