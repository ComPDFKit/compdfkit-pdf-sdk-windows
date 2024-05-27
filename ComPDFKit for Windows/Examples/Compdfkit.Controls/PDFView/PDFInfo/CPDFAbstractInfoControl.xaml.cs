using ComPDFKit.PDFDocument;
using ComPDFKit.Controls.Helper;
using ComPDFKitViewer;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFAbstractInfoControl : UserControl
    {
        public PDFViewControl ViewControl;
        public void InitWithPDFViewer(PDFViewControl viewControl)
        {
            this.ViewControl = viewControl;
            if(viewControl!=null && viewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer=viewControl.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc=pdfViewer?.GetDocument();
                if(pdfDoc!=null)
                {
                    InitializeAbstractInfo(pdfDoc);
                }
            }
        }

        public CPDFAbstractInfoControl()
        {
            InitializeComponent();
        }

        private void InitializeAbstractInfo(CPDFDocument cpdfDocument)
        {
            FileNameTextBlock.Text = cpdfDocument.FileName;
            FileSizeTextBlock.Text = CommonHelper.GetFileSize(cpdfDocument.FilePath);
            TitleTextBlock.Text = cpdfDocument.GetInfo().Title;
            AuthorTextBlock.Text = cpdfDocument.GetInfo().Author;
            SubjectTextBlock.Text = cpdfDocument.GetInfo().Subject;
            KeywordTextBlock.Text = cpdfDocument.GetInfo().Keywords;
        }
    }
}
