using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool;
using ComPDFKit.Controls.PDFControl;
using ComPDFKitViewer;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;

namespace ComPDFKit.Controls.PDFControlUI
{
    /// <summary>
    /// Interaction logic for CPDFImageUI.xaml
    /// </summary>
    public partial class CPDFTempStampUI : UserControl
    {
        private CPDFStampAnnotation stampAnnot;
        private PDFViewControl viewControl;

        public CPDFTempStampUI()
        {
            InitializeComponent();
        }

        public void SetPresentAnnotAttrib(StampParam param,CPDFStampAnnotation annot,CPDFDocument pdfDoc,PDFViewControl view)
        {
            stampAnnot = null;
            NoteTextBox.Text = param?.Content;
            stampAnnot=annot;
            viewControl = view;
            WriteableBitmap writeableBitmap = GetAnnotImage();
            CPDFAnnotationPreviewerControl.DrawStampPreview(writeableBitmap);
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(stampAnnot != null && stampAnnot.IsValid())
            {
                if (viewControl != null && viewControl.PDFViewTool != null)
                {
                    StampAnnotHistory history = new StampAnnotHistory();
                    history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, stampAnnot.Page.PageIndex, stampAnnot);
                    
                    stampAnnot.SetContent(NoteTextBox.Text);
                    stampAnnot.UpdateAp();
                    viewControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, stampAnnot.Page.PageIndex, stampAnnot);
                    viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
        }

        private WriteableBitmap GetAnnotImage()
        {
            if(stampAnnot!=null && stampAnnot.IsValid())
            {
                CRect rawRect=stampAnnot.GetRect();
                int width = (int)(rawRect.width() / 72D * 96D);
                int height = (int)(rawRect.height() / 72D * 96D);
                Rect rotateRect = new Rect(0, 0, width, height);
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.RotateAt(-90 * stampAnnot.Page.Rotation, width / 2, height / 2);
                rotateRect.Transform(rotateMatrix);
                byte[] ImageArray = new byte[(int)rotateRect.Width * (int)rotateRect.Height * 4];
                stampAnnot.RenderAnnot((int)rotateRect.Width, (int)rotateRect.Height, ImageArray);
                WriteableBitmap writeBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                writeBitmap.WritePixels(new Int32Rect(0, 0, width, height), ImageArray, width * 4, 0);
                return writeBitmap;
            }
            return null;
        }
    }
}
